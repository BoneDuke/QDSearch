
ALTER procedure [dbo].[mwCheckQuotesCycle]
--<VERSION>9.2.17</VERSION>
--<DATE>2012-11-28</DATE>
@pagingType	smallint,
@pageNum	int,		-- номер страницы(начиная с 1 или количество уже просмотренных записей для исключения при @pagingType=@ACTUALPLACES_PAGING)
@pageSize	int,
@agentKey	int,
@hotelQuotaMask smallint,
@aviaQuotaMask smallint,
@flightGroups	varchar(256),
@checkAgentQuota smallint,
@checkCommonQuota smallint,
@checkNoLongQuota smallint,
@requestOnRelease smallint,
@expiredReleaseResult int,
@noPlacesResult int,
@findFlight smallint = 0,	-- параметр устарел, вместо него используется признак подбора перелета. Оставлен для совместимости.
-- 4864 gorshkov
-- признак того, что мы подбираем варианты для подмешивания в поиск
@smartSearch bit = 0,
@tableName varchar(256) = null
as
begin
	-- настройки проверки квот через веб-сервис
	declare @checkQuotesOnWebService as bit, @checkQuotesService as nvarchar(150), @wasErrorCallingService bit
	set @checkQuotesOnWebService = 0
	set @wasErrorCallingService = 0
	select top 1 @checkQuotesOnWebService = ss_parmvalue from systemsettings with (nolock) where ss_parmname = 'NewSetToQuota'

	select top 1 @checkQuotesService = ss_parmvalue from systemsettings with (nolock) where ss_parmname = 'CheckQuotesWebService'

	if len(ltrim(rtrim(@checkQuotesService))) = 0 and @checkQuotesOnWebService = 1
		RAISERROR('mwCheckQuotesCycle: check quotes via webservice was enabled, but CheckQuotesWebService setting is not set in SystemSettings', 15, 1)

	declare @sAviaTariffFirst varchar(10), @sAviaTariffSecond varchar(10), 
	@nAviaTariffFirst smallint, @nAviaTariffSecond smallint
	
	declare @initialFindflight int
	set @initialFindflight = @findFlight

	declare @GREEN_LABEL smallint, @YELLOW_LABEL smallint, @RED_LABEL smallint
	set @GREEN_LABEL = 1
	set @YELLOW_LABEL = 4
	set @RED_LABEL = 2

	declare @step_index smallint, @price_correction int, @additional varchar(2000)
	
	if (@smartSearch = 1)
	begin
		-- хранит ключи отелей которые были подмешаны в поиск
		declare @smartSearchKeys table (hdKey int);
	end
	else
	begin
		-- настройка включающая SmartSearch
		declare @mwUseSmartSearch int
		select @mwUseSmartSearch=isnull(SS_ParmValue,0) from dbo.systemsettings 
		where SS_ParmName='mwUseSmartSearch'
		-- пока SmartSearch работает с только с ACTUALPLACES_PAGING
		if (@pagingType <> 2)
		begin
			set @mwUseSmartSearch = 0
		end
	end

	declare @mwCheckInnerAviaQuotes int
	select @mwCheckInnerAviaQuotes = isnull(SS_ParmValue,0) from dbo.systemsettings 
	where SS_ParmName = 'mwCheckInnerAviaQuotes'

	declare @DYNAMIC_SPO_PAGING smallint
	set @DYNAMIC_SPO_PAGING=3

	declare @tmpHotelQuota varchar(10), @tmpThereAviaQuota varchar(256), @tmpBackAviaQuota varchar(256), @allPlaces int,@places int,@actual smallint,@tmp varchar(256),
			@ptkey int,@pttourkey int, @ptpricekey bigint, @hdkey int,@rmkey int,@rckey int,@tourdate datetime,@chkey int,@chbackkey int,@hdday int,@hdnights int,@hdprkey int,	@chday int,@chpkkey int,@chprkey int,@chbackday int,
		@chbackpkkey int,@chbackprkey int,@days int, @rowNum int, @hdStep smallint, @reviewed int,@selected int, @hdPriceCorrection int, 
		@pt_directFlightAttribute int, @pt_backFlightAttribute int, @pt_mainplaces int, @pt_hrkey int, @sql varchar(max)

	declare @pt_chdirectkeys varchar(256), @pt_chbackkeys varchar(256)
	declare @tmpAllHotelQuota varchar(128),@pt_hddetails varchar(256)

	set @reviewed= @pageNum
	set @selected=0

	declare @now datetime, @percentPlaces float, @pos int
	declare @dateFrom datetime, @dateTo datetime
	set @now = getdate()
	set @pos = 0

	fetch next from quotaCursor into
	@ptkey,	
	@pttourkey,
	@ptpricekey,
	@hdkey,
	@rmkey,
	@rckey,
	@tourdate,
	@hdday,
	@hdnights,
	@hdprkey,
	@chday,
	@chpkkey,
	@chprkey,
	@chbackday,
	@chbackpkkey,
	@chbackprkey,
	@days,
	@chkey,
	@chbackkey,
	@rowNum, 
	@pt_chdirectkeys, 
	@pt_chbackkeys, 
	@pt_hddetails, 
	@pt_directFlightAttribute, 
	@pt_backFlightAttribute,
	@pt_mainplaces,
	@pt_hrkey

	while(@@fetch_status=0 and @selected < @pageSize)
	begin 
	
		if (@pos >= @pageNum 
		-- для подмешиваемых вариантов - интересует только одно размещение для каждого отеля
		and (@smartSearch = 0 or not exists (select top 1 1 from @smartSearchKeys where hdKey = @hdkey)))
		begin
			set @actual=1
			if(@aviaQuotaMask > 0)
			begin		
				declare @editableCode int
				set @editableCode = 2
				declare @isEditableService bit
				set @tmpThereAviaQuota=null
				if(@chkey > 0)
				begin 
					if @pt_directFlightAttribute is null
					begin
						--kadraliev MEG00025990 03.11.2010 Если в туре запрещено менять рейс, устанавливаем @findFlight = 0
						exec dbo.mwGetServiceIsEditableAttribute @pttourkey, @chkey, @chday, @days, @chprkey, @chpkkey, @isEditableService output
						if (@isEditableService = 0)
							set @pt_directFlightAttribute = 0
						else
							set @pt_directFlightAttribute = 2
						if (@tableName is not null)
						begin
							set @sql = 'update ' + @tableName + ' set pt_directFlightAttribute = ' + ltrim(str(@pt_directFlightAttribute)) + ' where pt_key = ' + ltrim(str(@ptkey))
							exec (@sql)
						end
					end
					set @findFlight = (@pt_directFlightAttribute & 2) / 2
					
					set @places=0
					EXEC [dbo].[mwCacheQuotaSearch] 1, @chkey, 0, 0, @tourdate, @chday, @days, @chprkey, @chpkkey, 
						@tmpThereAviaQuota OUTPUT, @places output, @step_index output, @price_correction output, @additional output, @findFlight

					if (@tmpThereAviaQuota is null)
					begin		
						
						set @tmpThereAviaQuota = ''

						exec dbo.mwCheckFlightGroupsQuotes @pagingType, @chkey, @flightGroups, @agentKey, @chprkey, @tourdate, @chday, 
							@requestOnRelease, @noPlacesResult, @checkAgentQuota, @checkCommonQuota, @checkNoLongQuota, @findFlight, @chpkkey,
							@days, @expiredReleaseResult, @aviaQuotaMask, @tmpThereAviaQuota output, @chbackday, @pt_mainplaces

						if len(ISNULL(@tmpThereAviaQuota, '')) != 0
						begin
							set @nAviaTariffFirst=0
							set @nAviaTariffSecond=0
							if len(@tmpThereAviaQuota)!=0
							BEGIN
								select 
									@sAviaTariffFirst = LEFT(@tmpThereAviaQuota,PATINDEX('%:%',@tmpThereAviaQuota)-1),
									@sAviaTariffSecond = LEFT(
									SUBSTRING(@tmpThereAviaQuota,PATINDEX('%|%',@tmpThereAviaQuota)+1,LEN(@tmpThereAviaQuota)-PATINDEX('%|%',@tmpThereAviaQuota)),
									PATINDEX('%:%',SUBSTRING(@tmpThereAviaQuota,PATINDEX('%|%',@tmpThereAviaQuota)+1,LEN(@tmpThereAviaQuota)-PATINDEX('%|%',@tmpThereAviaQuota)))-1)
								IF ISNUMERIC(@sAviaTariffFirst)=1
									set @nAviaTariffFirst=CAST(@sAviaTariffFirst as smallint)
								IF ISNUMERIC(@sAviaTariffSecond)=1
									set @nAviaTariffSecond=CAST(@sAviaTariffSecond as smallint)
								SET @places = abs(@nAviaTariffFirst)+abs(@nAviaTariffSecond)
							END

							EXEC [dbo].[mwCacheQuotaInsert] 1,@chkey,0,0,@tourdate,@chday,@days,@chprkey,@chpkkey,@tmpThereAviaQuota, @places, 0, 0, @additional, @findFlight
						end
					end		
					
					if len(@tmpThereAviaQuota)!=0
					begin
						-- проверка наличия мест на прямом перелете на соответствие маске квот
						-- проверяются все классы перелетов, если хотя бы один подходит - результат принимается
						declare @curIndex as int
						set @curIndex = 1
						
						declare @quota as varchar(260)
						set @quota = @tmpThereAviaQuota + '|'

						set @actual=0

						while @curIndex <= LEN(@quota)
						begin

							declare @freePlaces as int
							declare @freePlacesString as varchar(20)
							
							set @freePlaces = 0
							
							set @freePlacesString = SUBSTRING(@quota, @curIndex, CHARINDEX(':', @quota, @curIndex)-@curIndex)
							if ISNUMERIC(@freePlacesString) = 1
								set @freePlaces = CAST(@freePlacesString as smallint)
							
							set @curIndex = CHARINDEX('|', @quota, @curIndex)+1

							declare @freePlacesMask as int

							if @freePlaces = 0
								set @freePlacesMask = 2	-- no places
							else if @freePlaces < 0
								set @freePlacesMask = 4	-- request
							else
								set @freePlacesMask = 1	-- yes
								
							if (@aviaQuotaMask & @freePlacesMask) = @freePlacesMask
							begin
								-- прямой перелет удовлетворяет маске квот, прекращаем проверку
								set @actual=1
								break
							end
						end
					end
					else
						set @actual=0
				end
				if(@actual > 0)
				begin
					set @tmpBackAviaQuota=null
					if(@chbackkey > 0)
					begin
						if @pt_backFlightAttribute is null
						begin

							--karimbaeva MEG00038768 17.11.2011 получаем редактируемый атрибут услуги
							exec dbo.mwGetServiceIsEditableAttribute @pttourkey, @chbackkey, @chbackday, @days, @chbackprkey, @chbackpkkey, @isEditableService output
							if (@isEditableService = 0)
								set @pt_backFlightAttribute = 0
							else
								set @pt_backFlightAttribute = 2
							if (@tableName is not null)
							begin
								set @sql = 'update ' + @tableName + ' set pt_backFlightAttribute = ' + ltrim(str(@pt_backFlightAttribute)) + ' where pt_key = ' + ltrim(str(@ptkey))
								exec (@sql)
							end
		
						end	

						set @findFlight = (@pt_backFlightAttribute & 2) / 2

						EXEC [dbo].[mwCacheQuotaSearch] 1, @chbackkey, 0, 0, @tourdate, @chbackday, @days, @chbackprkey, @chbackpkkey, 
							@tmpBackAviaQuota OUTPUT, @places output, @step_index output, @price_correction output, @additional output, @findFlight
							
						if (@tmpBackAviaQuota is null)
						begin

							set @tmpBackAviaQuota = ''												
							
							exec dbo.mwCheckFlightGroupsQuotes @pagingType, @chbackkey, @flightGroups, @agentKey, @chbackprkey, @tourdate,
								@chbackday, @requestOnRelease, @noPlacesResult, @checkAgentQuota, @checkCommonQuota, @checkNoLongQuota, 
								@findFlight, @chbackpkkey, @days, @expiredReleaseResult, @aviaQuotaMask, @tmpBackAviaQuota output, @chday, @pt_mainplaces

							if len(ISNULL(@tmpBackAviaQuota, '')) != 0
							begin
								set @nAviaTariffFirst=0
								set @nAviaTariffSecond=0
								if len(@tmpBackAviaQuota)!=0
								BEGIN
									select 
									@sAviaTariffFirst = LEFT(@tmpBackAviaQuota,PATINDEX('%:%',@tmpBackAviaQuota)-1),
									@sAviaTariffSecond = LEFT(
									SUBSTRING(@tmpBackAviaQuota,PATINDEX('%|%',@tmpBackAviaQuota)+1,LEN(@tmpBackAviaQuota)-PATINDEX('%|%',@tmpBackAviaQuota)),
									PATINDEX('%:%',SUBSTRING(@tmpBackAviaQuota,PATINDEX('%|%',@tmpBackAviaQuota)+1,LEN(@tmpBackAviaQuota)-PATINDEX('%|%',@tmpBackAviaQuota)))-1)
									IF ISNUMERIC(@sAviaTariffFirst)=1
										set @nAviaTariffFirst=CAST(@sAviaTariffFirst as smallint)
									IF ISNUMERIC(@sAviaTariffSecond)=1
										set @nAviaTariffSecond=CAST(@sAviaTariffSecond as smallint)
									SET @places = abs(@nAviaTariffFirst)+abs(@nAviaTariffSecond)
								END
														
								EXEC [dbo].[mwCacheQuotaInsert] 1,@chbackkey,0,0,@tourdate,@chbackday,@days,@chbackprkey,@chbackpkkey,@tmpBackAviaQuota, @places, 0, 0, @additional, @findFlight
							end
						end

						if len(@tmpBackAviaQuota)!=0
						begin
							-- проверка наличия мест на обратном перелете на соответствие маске квот
							-- проверяются все классы перелетов, если хотя бы один подходит - результат принимается
							set @curIndex = 1						
							set @quota = @tmpBackAviaQuota + '|'
							set @actual=0

							while @curIndex <= LEN(@quota)
							begin
								
								set @freePlaces = 0
								
								set @freePlacesString = SUBSTRING(@quota, @curIndex, CHARINDEX(':', @quota, @curIndex)-@curIndex)
								if ISNUMERIC(@freePlacesString) = 1
									set @freePlaces = CAST(@freePlacesString as smallint)
								
								set @curIndex = CHARINDEX('|', @quota, @curIndex)+1
								if @freePlaces = 0
									set @freePlacesMask = 2	-- no places
								else if @freePlaces < 0
									set @freePlacesMask = 4	-- request
								else
									set @freePlacesMask = 1	-- yes
								
							if (@aviaQuotaMask & @freePlacesMask) = @freePlacesMask
								begin
									-- обратный перелет удовлетворяет маске квот, прекращаем проверку
									set @actual=1
									break
								end

							end
						
						end
						else
							set @actual=0				
							
					end
				end
			end			
			if(@hotelQuotaMask > 0)
			begin
				set @tmpAllHotelQuota = ''
				if(@actual > 0)
				begin
					if not (@pt_hddetails is not null and charindex(',', @pt_hddetails, 0) > 0)
					begin
						-- один отель
						set @tmpHotelQuota=null
						set @hdStep = 0
						set @hdPriceCorrection = 0
						set @places = 0

						EXEC [dbo].[mwCacheQuotaSearch] 3, @hdkey, @rmkey, @rckey, @tourdate, @hdday, @hdnights, @hdprkey, 0, 
							@tmpHotelQuota OUTPUT, @places output, @hdStep output, @hdPriceCorrection output, @additional output, 0

						if (@tmpHotelQuota is null)
						begin
							if @checkQuotesOnWebService = 1
							begin
								declare @checkQuotesResult as int
								set @dateFrom = dateadd(day, @hdday - 1, @tourdate)
								set @dateTo = dateadd(day, @hdnights - 1, @dateFrom)
								
								-- включена проверка квот через веб-сервис								
								begin try
									select @checkQuotesResult = result, @places = freePlaces, @allPlaces = allPlaces
									from [dbo].WcfQuotaCheckOneResult(
											1, 3, @hdkey, @pt_hrkey, @dateFrom, @dateTo, @hdprkey, 
											@agentKey, @hdnights, 1, null)
								end try
								begin catch
									-- Ошибка при вызове веб-сервиса. Логируем, отправляем письмо и отключаем проверку через сервис
									set @wasErrorCallingService = 1
								end catch
										
								if @checkQuotesResult in (0, 3)
									set @freePlacesMask = 2	-- no places
								else if @checkQuotesResult in (1, 2, 4)
								begin
									set @freePlacesMask = 4	-- request
									set @places = -1
								end
								else if @checkQuotesResult = 5
									set @freePlacesMask = 1	-- yes
								
							end
							
							-- не сделано через else к условию if @checkQuotesOnWebService = 1, чтобы в случае
							-- ошибки работы с веб-сервисом проверки квот
							if @wasErrorCallingService = 1 or @checkQuotesOnWebService = 0
							begin
								select @places=qt_places,@allPlaces=qt_allPlaces,@additional=qt_additional 
								from dbo.mwCheckQuotesEx(3,@hdkey,@rmkey,@rckey, @agentKey, @hdprkey,@tourdate,@hdday,@hdnights, 
									@requestOnRelease, @noPlacesResult, @checkAgentQuota, @checkCommonQuota, @checkNoLongQuota, 0, 0, 0, 0, 0,
									@expiredReleaseResult)

								--print '!!!'
								--print @hdkey
								--print @rmkey
								--print @rckey
								--print @agentKey
								--print @hdprkey
								--print @tourdate
								--print @hdday
								--print @hdnights
								--print @requestOnRelease
								--print @noPlacesResult
								--print @checkAgentQuota
								--print @checkCommonQuota
								--print @checkNoLongQuota
								--print 0
								--print 0
								--print 0
								--print 0
								--print 0
								--print @expiredReleaseResult
								--print '?'
								--print @places
								--print @allPlaces
								--print '?'
								--print '!!!'
									
								if @places = 0
									set @freePlacesMask = 2	-- no places
								else if @places < 0
								begin
									set @freePlacesMask = 4	-- request
								end
								else
									set @freePlacesMask = 1	-- yes
							end
							
							set @tmpHotelQuota=ltrim(str(@places)) + ':' + ltrim(str(@allPlaces))
							if(@pagingType = @DYNAMIC_SPO_PAGING and @places > 0)
							begin
								exec dbo.GetDynamicCorrections @now,@tourdate,3,@hdkey,@rmkey,@rckey,@places, @hdStep output, @hdPriceCorrection output
							end

							EXEC [dbo].[mwCacheQuotaInsert] 3,@hdkey,@rmkey,@rckey,@tourdate,@hdday,@hdnights,@hdprkey,0,@tmpHotelQuota,@places,@hdStep,@hdPriceCorrection, @additional, 0
						end
					end 
					else
					-----------------------------------------------
					--=== Check quotes for all hotels in tour ===--
					--===              [BEGIN]                -----
					begin
						set @places = 10000			-- первоначальное значение для дальнейшего сравнения и выбора наименьшего количества мест
													-- в многоотельном туре
					
						set @tmpAllHotelQuota = ''
						-- Mask for hotel details column :
						-- [HotelKey]:[RoomKey]:[RoomCategoryKey]:[HotelDay]:[HotelDays]:[HotelPartnerKey],...
						declare @curHotelKey int, @curRoomKey int , @curRoomCategoryKey int , @curHotelDay int , @curHotelDays int , @curHotelPartnerKey int
						declare @curHotelRoomKey as int

						declare @curHotelDetails varchar(256)
						declare @tempPlaces int
						declare @tempAllPlaces int
						declare @curPosition int
							set @curPosition = 0
						declare @prevPosition int
							set @prevPosition = 0
						declare @curHotelQuota  varchar(256)
						while (1 = 1)
						begin
							set @curPosition = charindex(',', @pt_hddetails, @curPosition + 1)
							if (@curPosition = 0)
								set @curHotelDetails  = substring(@pt_hddetails, @prevPosition, 256)
							else
								set @curHotelDetails  = substring(@pt_hddetails, @prevPosition, @curPosition - @prevPosition)
							
							-- Get details by current hotel
							begin try
								exec mwParseHotelDetails @curHotelDetails, @curHotelKey output, @curRoomKey output, @curRoomCategoryKey output, 
									@curHotelDay output, @curHotelDays output, @curHotelPartnerKey output, @curHotelRoomKey output
							end try
							begin catch
								--произошла ошибка, последующие отели просто не будут проверяться на наличие мест
								break
							end catch

							-----
							set @curHotelQuota = null
							EXEC [dbo].[mwCacheQuotaSearch] 3, @curHotelKey, @curRoomKey, @curRoomCategoryKey, @tourdate, @curHotelDay, @curHotelDays, @curHotelPartnerKey, 0, 
								@curHotelQuota OUTPUT, @tempPlaces output, @hdStep output, @hdPriceCorrection output, @additional output, 0

							print '!!!!!!!!!!!!!!!!!'
							print @curHotelQuota

							if (@curHotelQuota is null)
							begin								
								if @checkQuotesOnWebService = 1
								begin
									begin try
										set @dateFrom = dateadd(day, @curHotelDay - 1, @tourdate)
										set @dateTo = dateadd(day, @curHotelDays - 1, @dateFrom)
										-- включена проверка квот через веб-сервис
										select @checkQuotesResult = result, @tempPlaces = freePlaces, @tempAllPlaces = allPlaces
										from [dbo].WcfQuotaCheckOneResult(
												1, 3, @curHotelKey, @curHotelRoomKey, @dateFrom, @dateTo, @curHotelPartnerKey, 
												@agentKey, @curHotelDays, 1, null)
												
										-- отдельный случай для статуса "Запрос": сервис возвращает количество мест 0, а ожидается -1
										if @checkQuotesResult in (1, 2, 4)
											set @tempPlaces = -1
												
									end try
									begin catch
										set @wasErrorCallingService = 1
									end catch									
								end
								
								-- не сделано через else к условию if @checkQuotesOnWebService = 1, чтобы в случае
								-- ошибки работы с веб-сервисом проверки квот
								if @wasErrorCallingService = 1 or @checkQuotesOnWebService = 0
								begin
									print '!!!'
									print @curHotelKey
									print @curRoomKey
									print @curRoomCategoryKey
									print @agentKey
									print @curHotelPartnerKey
									print @tourdate
									print @curHotelDay
									print @curHotelDays
									print @requestOnRelease
									print @noPlacesResult
									print @checkAgentQuota
									print @checkCommonQuota
									print @checkNoLongQuota
									print 0
									print 0
									print 0
									print 0
									print 0
									print @expiredReleaseResult
									print '!!!'

									select @tempPlaces=qt_places,@tempAllPlaces=qt_allPlaces,@additional=qt_additional 
									from dbo.mwCheckQuotesEx(3,@curHotelKey,@curRoomKey,@curRoomCategoryKey, @agentKey, @curHotelPartnerKey,@tourdate,@curHotelDay,@curHotelDays, 
											@requestOnRelease, @noPlacesResult, @checkAgentQuota, @checkCommonQuota, @checkNoLongQuota, 0, 0, 0, 0, 0, @expiredReleaseResult)
								end
								
								set @curHotelQuota=ltrim(str(@tempPlaces)) + ':' + ltrim(str(@tempAllPlaces))

								print '??????????????????'
								print @curHotelQuota

								EXEC [dbo].[mwCacheQuotaInsert] 3,@curHotelKey,@curRoomKey,@curRoomCategoryKey,@tourdate,@curHotelDay,@curHotelDays,@curHotelPartnerKey,0,@curHotelQuota,@tempPlaces,0,0, @additional, 0
							end
							-----
							set @tmpAllHotelQuota = @tmpAllHotelQuota + @curHotelQuota + '|'
							
							if (@tempPlaces < @places or (@places < 0 and @tempPlaces = 0)) and not (@places = 0 and @tempPlaces < 0)
							begin
								
								-- @places - результирующее значение количества мест в текущей строке. Оно принимается как
								-- минимальное из всех отелей в случае многоотельного тура
								-- Условие написано с учетом того, что в данном случае -1 > 0 (нет мест - более сильный статус, чем запрос)
								set @places = @tempPlaces
								set @tmpHotelQuota = @curHotelQuota

							end

							if (@curPosition = 0)
								break
							set @prevPosition = @curPosition + 1
						end
						
						-- Remove comma at the end of string
						if(len(@tmpAllHotelQuota) > 0)
							set @tmpAllHotelQuota = substring(@tmpAllHotelQuota, 1, len(@tmpAllHotelQuota) - 1)
					end
					--===                [END]                -----
					--=== Check quotes for all hotels in tour ===--
					-----------------------------------------------
					
					if @places = 0
						set @freePlacesMask = 2	-- no places
					else if @places < 0
					begin
						set @freePlacesMask = 4	-- request
					end
					else
						set @freePlacesMask = 1	-- yes
							
					if (@hotelQuotaMask & @freePlacesMask) = @freePlacesMask
						set @actual = 1
					else
						set @actual = 0
					
					--if((@places > 0 and (@hotelQuotaMask & 1)=0) or (@places=0 and (@hotelQuotaMask & 2)=0) or (@places=-1 and (@hotelQuotaMask & 4)=0))
					--	set @actual=0
				end
			end



	------==============================================================================================------
	--============================ Check inner avia quotes if needed by settings ===========================--
	--========																						========--
			if(@actual > 0 and @mwCheckInnerAviaQuotes > 0)
			begin
				-- Direct flights
				if (@pt_chdirectkeys is not null and charindex(',', @pt_chdirectkeys, 0) > 0)
				begin
					set @findFlight = @initialFindflight
					exec dbo.mwCheckFlightGroupsQuotesWithInnerFlights @pagingType, @pt_chdirectkeys, 
							@flightGroups, @agentKey, @tourdate, @requestOnRelease, @noPlacesResult, 
							@checkAgentQuota, @checkCommonQuota, @checkNoLongQuota, @findFlight, 
							@days, @expiredReleaseResult, @aviaQuotaMask, @tmpThereAviaQuota output, @pt_chbackkeys
					if (len(@tmpThereAviaQuota) = 0)
						set @actual = 0
				end 

				-- Back flights
				if(@actual > 0)
				begin
					if (@pt_chbackkeys is not null and charindex(',', @pt_chbackkeys, 0) > 0)
					begin
						set @findFlight = @initialFindflight
						exec dbo.mwCheckFlightGroupsQuotesWithInnerFlights @pagingType, @pt_chbackkeys,   
							@flightGroups, @agentKey, @tourdate, @requestOnRelease, @noPlacesResult, 
							@checkAgentQuota, @checkCommonQuota, @checkNoLongQuota, @findFlight, 
							@days, @expiredReleaseResult, @aviaQuotaMask, @tmpBackAviaQuota output, @pt_chdirectkeys
						if (len(@tmpBackAviaQuota) = 0)
							set @actual = 0
					end 
				end
			end
	--========																						========--
	--============================                                               ===========================--
	------==============================================================================================------
			
			if(@actual > 0)
			begin
				if (@smartSearch = 1)
				begin
					-- сохраним ключ отеля для которого уже было добавлено размещение
					insert into @smartSearchKeys(hdKey) values (@hdkey)
					set @selected=@selected + 1
					-- pt_smartSearch = 1 (для выделения подмешанных вариантов)
					insert into #Paging(ptKey,pt_hdquota,pt_chtherequota,pt_chbackquota,chkey,chbackkey,stepId,priceCorrection, pt_hdallquota, pt_smartSearch)
					values(@ptkey,@tmpHotelQuota,@tmpThereAviaQuota,@tmpBackAviaQuota,@chkey,@chbackkey,@hdStep,@hdPriceCorrection, @tmpAllHotelQuota, 1)
				end
				-- если используется SmartSearch (глобально - включена настройка, но mwCheckQuotesCycle вызвана НЕ для подмешанных вариантов) 
				-- то возможна ситуация когда данный ptKey уже был добавлен в #Paging как подмешанный
				else if (@mwUseSmartSearch = 0 or not exists (select top 1 1 from #Paging where ptKey = @ptkey))
				begin
					set @selected=@selected + 1
					insert into #Paging(ptKey,ptpricekey,pt_hdquota,pt_chtherequota,pt_chbackquota,chkey,chbackkey,stepId,priceCorrection, pt_hdallquota)
					values(@ptkey,@ptpricekey,@tmpHotelQuota,@tmpThereAviaQuota,@tmpBackAviaQuota,@chkey,@chbackkey,@hdStep,@hdPriceCorrection, @tmpAllHotelQuota)
				end
			end

			set @reviewed=@reviewed + 1
		end
		fetch next from quotaCursor into @ptkey,@pttourkey,@ptpricekey,@hdkey,@rmkey,@rckey,@tourdate,@hdday,@hdnights,@hdprkey,@chday,@chpkkey,
			@chprkey,@chbackday,@chbackpkkey,@chbackprkey,@days,@chkey,@chbackkey,@rowNum, @pt_chdirectkeys, @pt_chbackkeys, 
			@pt_hddetails, @pt_directFlightAttribute, @pt_backFlightAttribute, @pt_mainplaces, @pt_hrkey
		set @pos = @pos + 1
	end

	if (@smartSearch=0)
	begin
		select @reviewed
	end
end
