if exists (select name from sys.objects where name like 'FillMasterWebSearchFields' and type = 'p')
begin
	DROP PROCEDURE [dbo].[FillMasterWebSearchFields]
end
GO

create procedure [dbo].[FillMasterWebSearchFields](@tokey int, @calcKey int = null, @forceEnable smallint = null, @overwritePrices bit = null)
-- if @forceEnable > 0 (by default) then make call mwEnablePriceTour @calcKey, 1 at the end of the procedure
as
begin
	--<VERSION>2009.2.21.1</VERSION>
	--<DATE>2014-04-10</DATE>
	set @forceEnable = isnull(@forceEnable, 1)

	declare @findByAdultChild int, @newRecalcPrice int
	
	declare @counter int, @deleteCount int, @params nvarchar(500)
	
	set @findByAdultChild = isnull((select top 1 convert(int, SS_ParmValue) from SystemSettings where SS_ParmName = 'OnlineFindByAdultChild'), 0)
	set @newRecalcPrice = isnull((select top 1 convert(int, SS_ParmValue) from SystemSettings where SS_ParmName = 'NewReCalculatePrice'), 0)

	if (@tokey is null)
	begin
		print 'Procedure does not support NULL param. You must specify @tokey parameter.'
		return
	end

	DECLARE @departFromKey INT
	SELECT top 1 @departFromKey = TL_CTDepartureKey FROM tbl_TurList 
	INNER JOIN tp_Tours 
	ON TL_KEY = TO_TRKey
	WHERE TO_Key = @tokey
	
	IF EXISTS(SELECT 1 FROM mwSpoDataTable WHERE sd_tourkey = @tokey AND sd_ctkeyfrom <> @departFromKey)
	BEGIN
		SET @calcKey = null
		EXEC mwReplDisablePriceTour @tokey
	END

	update dbo.TP_Tours set TO_Progress = 0 where TO_Key = @tokey

	if dbo.mwReplIsSubscriber() > 0
	begin
		exec dbo.mwFillTP @tokey, @calcKey
	end

	create table #tmpHotelData (
		thd_tourkey int, 
		thd_firsthdkey int,
		thd_firstpnkey int, 
		thd_cnkey int, 
		thd_tlkey int, 
		thd_isenabled smallint, 
		thd_tourcreated datetime, 
		thd_hdstars nvarchar(15), 
		thd_ctkey int, 
		thd_rskey int, 
		thd_hdkey int, 
		thd_hdpartnerkey int, 
		thd_hrkey int, 
		thd_rmkey int, 
		thd_rckey int, 
		thd_ackey int, 
		thd_pnkey int, 
		thd_hdmain smallint,
		thd_firsthotelday int,
		thd_ctkeyfrom int, 
		thd_ctkeyto int, 
		thd_apkeyfrom int, 
		thd_apkeyto int,
		thd_tourtype int,
		thd_cnname nvarchar(200) collate database_default,
		thd_tourname nvarchar(200) collate database_default,
		thd_hdname nvarchar(200) collate database_default,
		thd_ctname nvarchar(200) collate database_default,
		thd_rsname nvarchar(200) collate database_default,
		thd_ctfromname nvarchar(200) collate database_default,
		thd_cttoname nvarchar(200) collate database_default,
		thd_tourtypename nvarchar(200) collate database_default,
		thd_pncode nvarchar(50) collate database_default,
		thd_hdorder int,
		thd_hotelkeys nvarchar(256) collate database_default,
		thd_pansionkeys nvarchar(256) collate database_default,
		thd_hotelnights nvarchar(256) collate database_default,
		thd_tourvalid datetime,
		thd_hotelurl varchar(254) collate database_default
	)

	-- создадим темповую ценовую таблицу
	select top 1 * into #tempPriceTable from mwPriceDataTable with(nolock)
	truncate table #tempPriceTable
	
	
	CREATE NONCLUSTERED INDEX [x_main] ON [dbo].[#tempPriceTable] 
	(
		pt_tourdate asc,
		pt_hdkey asc,
		pt_rmkey asc,
		pt_rckey asc,
		pt_ackey asc,
		pt_pnkey asc,
		pt_days asc,
		pt_nights asc,
		pt_tourtype asc,
		pt_ctkeyfrom asc
	)

	select top 1
		ti_key,
		ti_tokey,
		ti_firsthdkey,
		ti_firstpnkey,
		ti_firsthrkey,
		ti_firsthotelday,
		ti_lasthotelday,
		ti_totaldays,
		ti_nights,
		ti_hotelkeys,
		ti_hotelroomkeys,
		ti_hoteldays,
		ti_hotelstars,
		ti_pansionkeys,
		ti_hdpartnerkey,
		ti_firsthotelpartnerkey,
		ti_hdday,
		ti_hdnights,
		ti_chkey,
		ti_chday,
		ti_chpkkey,
		ti_chprkey,
		ti_ctkeyfrom,
		ti_chbackkey,
		ti_chbackday,
		ti_chbackpkkey,
		ti_chbackprkey,
		ti_ctkeyto,
		ti_apkeyfrom,
		ti_apkeyto,
		ti_firstctkey,
		ti_firstrskey,
		ti_firsthdstars
	into #tp_lists
	from tp_lists with(nolock)

	truncate table #tp_lists
	alter table #tp_lists add primary key(ti_key)
	alter table #tp_lists add ti_roomtypekeys varchar(256)

	-- Город отправления из свойств тура
	declare @ctdeparturekey int
	select	@ctdeparturekey = tl_ctdeparturekey
	from	tp_tours with(nolock)
		inner join tbl_turList with(nolock) on tbl_turList.tl_key = to_trkey
	where to_key = @tokey

	if (@ctdeparturekey is null or @ctdeparturekey = 0)
	begin
		-- Подбираем город вылета первого рейса
		exec GetCityDepartureKey @tokey, @ctdeparturekey output
	end

	declare @firsthdday int
	select @firsthdday = (select min(ts_day) 
				from tp_services with (nolock)
 				where ts_svkey = 3 and ts_tokey = @tokey)

	declare @count_ts_code int

	select @count_ts_code = count(distinct ts_code)
	from tp_services with(nolock)
	where ts_svkey = 1 and ts_tokey = @tokey 
	and (ts_day <= @firsthdday or (ts_day = 1 and @firsthdday = 0)) 
	and ts_subcode2 = @ctdeparturekey

	if (@count_ts_code > 1)
	begin
		if(@calcKey is not null)
		begin
			insert into #tp_lists
			select
				ti_key,
				ti_tokey,
				ti_firsthdkey,
				ti_firstpnkey,
				ti_firsthrkey,
				@firsthdday as ti_firsthotelday,
				ti_lasthotelday,
				ti_totaldays,
				ti_nights,
				ti_hotelkeys,
				ti_hotelroomkeys,
				ti_hoteldays,
				ti_hotelstars,
				ti_pansionkeys,
				ti_hdpartnerkey,
				ti_firsthotelpartnerkey,
				ti_hdday,
				ti_hdnights,
				(
					select top 1 ts_code
					from tp_servicelists with(nolock) 
					inner join tp_services with(nolock) on tl_tskey = ts_key and ts_svkey = 1
					where tl_tikey = ti_key and ts_tokey = @tokey and tl_tokey = @tokey 
					and (ts_day <= @firsthdday or (ts_day = 1 and @firsthdday = 0)) and ts_subcode2 = @ctdeparturekey
				) as ti_chkey,
				ti_chday,
				ti_chpkkey,
				ti_chprkey,
				ti_ctkeyfrom,
				ti_chbackkey,
				ti_chbackday,
				ti_chbackpkkey,
				ti_chbackprkey,
				ti_ctkeyto,
				ti_apkeyfrom,
				ti_apkeyto,
				ti_firstctkey,
				ti_firstrskey,
				ti_firsthdstars,
				null
			from tp_lists with(nolock)
			where TI_Key in (select TP_TIKey from TP_Prices with(nolock) where TP_TOKey = TI_TOKey and TP_CalculatingKey = @calcKey) 
			and TI_TOKey = @tokey
		end
		else
		begin
			insert into #tp_lists
			select
				ti_key,
				ti_tokey,
				ti_firsthdkey,
				ti_firstpnkey,
				ti_firsthrkey,
				@firsthdday as ti_firsthotelday,
				ti_lasthotelday,
				ti_totaldays,
				ti_nights,
				ti_hotelkeys,
				ti_hotelroomkeys,
				ti_hoteldays,
				ti_hotelstars,
				ti_pansionkeys,
				ti_hdpartnerkey,
				ti_firsthotelpartnerkey,
				ti_hdday,
				ti_hdnights,
				(
					select top 1 ts_code
					from tp_servicelists with(nolock) 
					inner join tp_services with(nolock) on tl_tskey = ts_key and ts_svkey = 1
					where tl_tikey = ti_key and ts_tokey = @tokey and tl_tokey = @tokey 
					and (ts_day <= @firsthdday or (ts_day = 1 and @firsthdday = 0)) and ts_subcode2 = @ctdeparturekey
				) as ti_chkey,	
				ti_chday,
				ti_chpkkey,
				ti_chprkey,
				ti_ctkeyfrom,
				ti_chbackkey,
				ti_chbackday,
				ti_chbackpkkey,
				ti_chbackprkey,
				ti_ctkeyto,
				ti_apkeyfrom,
				ti_apkeyto,
				ti_firstctkey,
				ti_firstrskey,
				ti_firsthdstars,
				null
			from tp_lists with(nolock)
			where TI_TOKey = @tokey		
		end
	end
	else
	begin

		declare @ts_code int
		declare @ti_key int
		select top 1 @ti_key = ti_key
		from tp_lists with(nolock)
		where TI_TOKey = @tokey	

		select top 1 @ts_code = ts_code
		from tp_services with(nolock)
		where ts_svkey = 1 and ts_tokey = @tokey
		and (ts_day <= @firsthdday or (ts_day = 1 and @firsthdday = 0)) 
		and ts_subcode2 = @ctdeparturekey

		if(@calcKey is not null)
		begin
			insert into #tp_lists
			select
				ti_key,
				ti_tokey,
				ti_firsthdkey,
				ti_firstpnkey,
				ti_firsthrkey,
				@firsthdday as ti_firsthotelday,
				ti_lasthotelday,
				ti_totaldays,
				ti_nights,
				ti_hotelkeys,
				ti_hotelroomkeys,
				ti_hoteldays,
				ti_hotelstars,
				ti_pansionkeys,
				ti_hdpartnerkey,
				ti_firsthotelpartnerkey,
				ti_hdday,
				ti_hdnights,
				@ts_code as ti_chkey,			
				ti_chday,
				ti_chpkkey,
				ti_chprkey,
				ti_ctkeyfrom,
				ti_chbackkey,
				ti_chbackday,
				ti_chbackpkkey,
				ti_chbackprkey,
				ti_ctkeyto,
				ti_apkeyfrom,
				ti_apkeyto,
				ti_firstctkey,
				ti_firstrskey,
				ti_firsthdstars,
				null
			from tp_lists with(nolock)
			where TI_Key in (select TP_TIKey from TP_Prices with(nolock) where TP_TOKey = TI_TOKey and TP_CalculatingKey = @calcKey) 
			and TI_TOKey = @tokey
		end
		else
		begin
			insert into #tp_lists
			select
				ti_key,
				ti_tokey,
				ti_firsthdkey,
				ti_firstpnkey,
				ti_firsthrkey,
				@firsthdday as ti_firsthotelday,
				ti_lasthotelday,
				ti_totaldays,
				ti_nights,
				ti_hotelkeys,
				ti_hotelroomkeys,
				ti_hoteldays,
				ti_hotelstars,
				ti_pansionkeys,
				ti_hdpartnerkey,
				ti_firsthotelpartnerkey,
				ti_hdday,
				ti_hdnights,
				@ts_code as ti_chkey,			
				ti_chday,
				ti_chpkkey,
				ti_chprkey,
				ti_ctkeyfrom,
				ti_chbackkey,
				ti_chbackday,
				ti_chbackpkkey,
				ti_chbackprkey,
				ti_ctkeyto,
				ti_apkeyfrom,
				ti_apkeyto,
				ti_firstctkey,
				ti_firstrskey,
				ti_firsthdstars,
				null
			from tp_lists with(nolock)
			where TI_TOKey = @tokey		
		end
	end

	declare @mwAccomodationPlaces nvarchar(254)
	declare @mwRoomsExtraPlaces nvarchar(254)
	declare @mwSearchType int
	declare @sql nvarchar(4000)
	declare @countryKey int
	declare @cityFromKey int

	update dbo.TP_Tours set TO_Progress = 7 where TO_Key = @tokey

	update TP_Tours set TO_MinPrice = (
			select min(TP_Gross) 
			from TP_Prices with(nolock) 
				left join TP_Lists with(nolock) on ti_key = tp_tikey
				left join HotelRooms with(nolock) on hr_key = ti_firsthrkey				
			where TP_TOKey = TO_Key 
					and hr_main > 0 
					and (isnull(HR_AGEFROM, 0) <= 0 or isnull(HR_AGEFROM, 0) > 16)
		)
		where TO_Key = @tokey

	update dbo.TP_Tours set TO_Progress = 13 where TO_Key = @tokey

	update #tp_lists
	set
		ti_lasthotelday = (select max(ts_day)
				from tp_servicelists  with (nolock)
					inner join tp_services with (nolock) on tl_tskey = ts_key
				where tl_tikey = ti_key and ts_svkey = 3 and TS_TOKey = @tokey and TL_TOKey = @tokey)

	update dbo.TP_Tours set TO_Progress = 20 where TO_Key = @tokey	

	update dbo.TP_Tours set TO_Progress = 30 where TO_Key = @tokey

	-- MEG00024548 Paul G 11.01.2009
	-- изменил логику подсчёта кол-ва ночей в туре
	-- раньше было сумма ночей проживания по всем отелям в туре
	-- теперь если проживания пересекаются, лишние ночи не суммируются
	update #tp_lists 
	set
		ti_nights = dbo.mwGetTiNights(ti_key)

	--koshelev
	--02.04.2012 MEG00040744
    declare @result nvarchar(256)
    set @result = N''
    select @result = @result + rtrim(ltrim(str(tbl.ti_nights))) + N', ' from (select distinct ti_nights from (select ti_nights from #tp_lists union select ti_nights from tp_lists with(nolock) where ti_tokey = @tokey ) as tbl2) tbl order by tbl.ti_nights
    declare @len int
    set @len = len(@result)
    if(@len > 0)
          set @result = substring(@result, 1, @len - 1)

    update TP_Tours set TO_HotelNights = @result where TO_Key = @tokey

	update dbo.TP_Tours set TO_Progress = 40 where TO_Key = @tokey

	update #tp_lists 
		set ti_hotelkeys = dbo.mwGetTiHotelKeys(ti_key),
			ti_hotelroomkeys = dbo.mwGetTiHotelRoomKeys(ti_key),
			ti_roomtypekeys = dbo.mwGetTiRoomTypeKeys(ti_key),
			ti_hoteldays = dbo.mwGetTiHotelNights(ti_key),
			ti_hotelstars = dbo.mwGetTiHotelStars(ti_key),
			ti_pansionkeys = dbo.mwGetTiPansionKeys(ti_key)

	update #tp_lists
	set
		ti_hdpartnerkey = ts_oppartnerkey,
		ti_firsthotelpartnerkey = ts_oppartnerkey,
		ti_hdday = ts_day,
		ti_hdnights = ts_days
	from tp_servicelists with (nolock)
		inner join tp_services with (nolock) on (tl_tskey = ts_key and ts_svkey = 3)
	where tl_tikey = ti_key and ts_code = ti_firsthdkey and TS_TOKey = @tokey and TL_TOKey = @tokey

	update dbo.TP_Tours set TO_Progress = 50 where TO_Key = @tokey

	-- город вылета + прямой перелет
	update #tp_lists
	set 
		ti_chday = ts_day,
		ti_chpkkey = ts_oppacketkey,
		ti_chprkey = ts_oppartnerkey
	from tp_servicelists with(nolock) inner join tp_services with(nolock) on tl_tskey = ts_key and ts_svkey = 1
	where	tl_tikey = ti_key 
		and (ts_day <= ti_firsthotelday or (ts_day = 1 and ti_firsthotelday = 0))
		and ts_code = ti_chkey 
		and ts_subcode2 = @ctdeparturekey
		and TS_TOKey = @tokey and TL_TOKey = @tokey

	update #tp_lists
	set 
		ti_ctkeyfrom = @ctdeparturekey

	-- Проверка наличия перелетов в город вылета
	declare @existBackCharter smallint
	select	@existBackCharter = count(ts_key)
	from	tp_services with(nolock)
	where	ts_tokey = @tokey
		and	ts_svkey = 1
		and ts_ctkey = @ctdeparturekey

	-- город прилета + обратный перелет
	update #tp_lists
	set 
		ti_chbackkey = ts_code,
		ti_chbackday = ts_day,
		ti_chbackpkkey = ts_oppacketkey,
		ti_chbackprkey = ts_oppartnerkey,
		ti_ctkeyto = ts_subcode2
	from tp_servicelists with(nolock)
		inner join tp_services with(nolock) on (tl_tskey = ts_key and ts_svkey = 1)
		inner join tp_tours with(nolock) on ts_tokey = to_key 
	where 
		tl_tikey = ti_key 
		and ts_day > ti_lasthotelday
		and (ts_ctkey = @ctdeparturekey or @existBackCharter = 0)
		and TI_TOKey = @tokey
		and TS_TOKey = @tokey and TL_TOKey = @tokey

	-- _ключ_ аэропорта вылета
	update #tp_lists 
	set 
		ti_apkeyfrom = (select top 1 ap_key from airport with(nolock), charter with(nolock) 
				where ch_portcodefrom = ap_code 
					and ch_key = ti_chkey)

	-- _ключ_ аэропорта прилета
	update #tp_lists
	set 
		ti_apkeyto = (select top 1 ap_key from airport with(nolock), charter with(nolock) 
				where ch_portcodefrom = ap_code 
					and ch_key = ti_chbackkey)

	-- ключ города и ключ курорта + звезды
	update #tp_lists
	set
		ti_firstctkey = hd_ctkey,
		ti_firstrskey = hd_rskey,
		ti_firsthdstars = hd_stars
	from hoteldictionary with(nolock)
	where 
		ti_firsthdkey = hd_key

	update dbo.TP_Tours set TO_Progress = 60 where TO_Key = @tokey

	if dbo.mwReplIsPublisher() > 0
	begin
		declare @trkey int
		select @trkey = to_trkey from dbo.tp_tours with(nolock) where to_key = @tokey
		
		insert into dbo.mwReplTours (rt_trkey, rt_tokey, rt_date, rt_CalcKey)
		values (@trkey, @tokey, getdate(), @calcKey)
		
		update CalculatingPriceLists set CP_Status = 0 where CP_PriceTourKey = @tokey
		update dbo.TP_Tours 
		set TO_Update = 0, 
			TO_Progress = 100,
			TO_IsEnabled = 1
		where TO_Key = @tokey
		
		--return
	end

	-- временная таблица с информацией об отелях
	insert into #tmpHotelData (
		thd_tourkey, 
		thd_firsthdkey, 
		thd_firstpnkey, 
		thd_cnkey, 
		thd_tlkey, 
		thd_isenabled, 
		thd_tourcreated, 
		thd_hdstars, 
		thd_ctkey, 
		thd_rskey, 
		thd_hdkey, 
		thd_hdpartnerkey, 
		thd_hrkey, 
		thd_rmkey, 
		thd_rckey, 
		thd_ackey, 
		thd_pnkey, 
		thd_hdmain,
		thd_firsthotelday,
		thd_ctkeyfrom, 
		thd_ctkeyto, 
		thd_apkeyfrom, 
		thd_apkeyto,
		thd_tourtype,
		thd_cnname,
		thd_tourname,
		thd_hdname,
		thd_ctname,
		thd_rsname,
		thd_ctfromname,
		thd_cttoname,
		thd_tourtypename,
		thd_pncode,
		thd_hotelkeys,
		thd_pansionkeys,
		thd_hotelnights,
		thd_tourvalid,
		thd_hotelurl
	)
	select distinct 
		to_key, 
		ti_firsthdkey, 
		ti_firstpnkey,
		to_cnkey, 
		to_trkey, 
		@forceEnable, 
		to_datecreated, 
		hd_stars, 
		hd_ctkey, 
		hd_rskey, 
		ts_code, 
		ts_oppartnerkey, 
		ts_subcode1, 
		hr_rmkey, 
		hr_rckey, 
		hr_ackey, 
		ts_subcode2, 
		(case ts_code when ti_firsthdkey then 1 else 0 end),
		ti_firsthotelday,
		isnull(ti_ctkeyfrom, 0), 
		ti_ctkeyto, 
		ti_apkeyfrom, 
		ti_apkeyto,
		tl_tip,
		cn_name,
		isnull(tl_nameweb, isnull(to_name, tl_name)),
		hd_name,
		ct_name,
		null,
		null,
		null,
		tp_name,
		pn_code,
		ti_hotelkeys,
		ti_pansionkeys,
		ti_hoteldays,
		to_datevalid,
		hd_http
	from #tp_lists with(nolock)
		inner join tp_tours with(nolock) on ti_tokey = to_key
		inner join tp_servicelists with(nolock) on tl_tikey = ti_key 
		inner join tp_services with(nolock) on (tl_tskey = ts_key and ts_svkey = 3) 
		inner join hoteldictionary with(nolock) on ts_code = hd_key
		inner join hotelrooms with(nolock) on hr_key = ts_subcode1
		inner join turList with(nolock) on turList.tl_key = to_trkey
		inner join country with(nolock) on cn_key = to_cnkey
		inner join citydictionary with(nolock) on ct_key = hd_ctkey
		inner join tiptur with(nolock) on tp_key = tl_tip
		inner join pansion with(nolock) on pn_key = ts_subcode2
	where to_key = @tokey and to_datevalid >= getdate() 
		and TS_TOKey = @tokey and TL_TOKey = @tokey

	update #tmpHotelData set thd_hdorder = (select min(ts_day) from tp_services with(nolock) where ts_tokey = thd_tourkey and ts_svkey = 3 and ts_code = thd_hdkey)
	update #tmpHotelData set thd_rsname = rs_name from resorts with(nolock) where rs_key = thd_rskey
	update #tmpHotelData set thd_ctfromname = ct_name from citydictionary with(nolock) where ct_key = thd_ctkeyfrom
	update #tmpHotelData set thd_ctfromname = '-Без перелета-' where thd_ctkeyfrom = 0
	update #tmpHotelData set thd_cttoname = ct_name from citydictionary with(nolock) where ct_key = thd_ctkeyto
	update #tmpHotelData set thd_cttoname = '-Без перелета-' where thd_ctkeyto = 0
	--

	update dbo.TP_Tours set TO_Progress = 70 where TO_Key = @tokey

	select @mwAccomodationPlaces = ltrim(rtrim(isnull(SS_ParmValue, ''))) from dbo.systemsettings with(nolock)
	where SS_ParmName = 'MWAccomodationPlaces'

	select @mwRoomsExtraPlaces = ltrim(rtrim(isnull(SS_ParmValue, ''))) from dbo.systemsettings with(nolock) 
	where SS_ParmName = 'MWRoomsExtraPlaces'

	select @mwSearchType = isnull(SS_ParmValue, 1) from dbo.systemsettings with(nolock) 
	where SS_ParmName = 'MWDivideByCountry'

	if (@calcKey is null)
	begin
		delete from dbo.mwSpoDataTable where sd_tourkey = @tokey
		delete from dbo.mwPriceHotels where sd_tourkey = @tokey
		delete from dbo.mwPriceDurations where sd_tourkey = @tokey
	end
	else
	begin
		--saifullina 16.01.2013 если мы изменили название и дозаписываем тур, то должны дозаписать с новым названием
		update dbo.mwSpoDataTable set sd_tourname=(select to_name from TP_Tours with(nolock) where TO_Key=@tokey) where sd_tourkey = @tokey
	end

	--MEG00026692 Paul G 25.03.2010
	--функции от ti_key должны вызываться на каждую запись из tp_lists
	--поэтому результаты их выполнения записываю в темповую таблицу
	--которую джоиню в последующем селекте
	create table #tempTourInfo (
		tt_tikey int,
		tt_charterto varchar(256) collate database_default,
		tt_charterback varchar(256) collate database_default,
		tt_tourhotels varchar(256) collate database_default,
		tt_directFlightAttribute int,
		tt_backFlightAttribute int
	)

	insert into #tempTourInfo
	(
		tt_tikey, 
		tt_charterto, 
		tt_charterback, 
		tt_tourhotels,
		tt_directFlightAttribute,
		tt_backFlightAttribute
	)
	select 
		ti_key, 
		dbo.mwGetTourCharters(ti_key, 1), 
		dbo.mwGetTourCharters(ti_key, 0), 
		dbo.mwGetTourHotels(ti_key),
		dbo.mwGetTourCharterAttribute(ti_key, 1),
		dbo.mwGetTourCharterAttribute(ti_key, 0)
	from #tp_lists with(nolock)
	--End MEG00026692	

	if(@calcKey is not null)
	begin
		insert into #tempPriceTable (
			[pt_mainplaces],
			[pt_addplaces],
			[pt_main],
			[pt_tourvalid],
			[pt_tourcreated],
			[pt_tourdate],
			[pt_days],
			[pt_nights],
			[pt_cnkey],
			[pt_ctkeyfrom],
			[pt_apkeyfrom],
			[pt_ctkeyto],
			[pt_apkeyto],
			[pt_ctkeybackfrom],
			[pt_ctkeybackto],
			[pt_tourkey],
			[pt_tourtype],
			[pt_tlkey],
			[pt_pricelistkey],
			[pt_pricekey],
			[pt_price],
			[pt_hdkey],
			[pt_hdpartnerkey],
			[pt_rskey],
			[pt_ctkey],
			[pt_hdstars],
			[pt_pnkey],
			[pt_hrkey],
			[pt_rmkey],
			[pt_rckey],
			[pt_ackey],
			[pt_childagefrom],
			[pt_childageto],
			[pt_childagefrom2],
			[pt_childageto2],
			[pt_hdname],
			[pt_tourname],
			[pt_pnname],
			[pt_pncode],
			[pt_rmname],
			[pt_rmcode],
			[pt_rcname],
			[pt_rccode],
			[pt_acname],
			[pt_accode],
			[pt_rsname],
			[pt_ctname],
			[pt_rmorder],
			[pt_rcorder],
			[pt_acorder],
			[pt_rate],
			[pt_toururl],
			[pt_hotelurl],
			[pt_isenabled],
			[pt_chkey],
			[pt_chbackkey],
			[pt_hdday],
			[pt_hdnights],
			[pt_chday],
			[pt_chpkkey],
			[pt_chprkey],
			[pt_chbackday],
			[pt_chbackpkkey],
			[pt_chbackprkey],
			pt_hotelkeys,
			pt_hotelroomkeys,
			pt_roomtypekeys,
			pt_hotelstars,
			pt_pansionkeys,
			pt_hotelnights,
			pt_chdirectkeys,
			pt_chbackkeys,
			[pt_topricefor],
			pt_tlattribute,
			pt_hddetails,
			pt_directFlightAttribute,
			pt_backFlightAttribute
		)
		select 
				(	case when @mwAccomodationPlaces = '0'
					then isnull(rm_nplaces, 0)
					else (	case when @findByAdultChild = 1 -- искать по взрослым
							then isnull(AC_NADMAIN, 0) + isnull(AC_NADEXTRA,0)
							-- искать по основным
							else isnull(AC_NADMAIN, 0) + isnull(AC_NCHMAIN, 0)
							end)
					end),
				(	case when isnull(ac_nmenexbed, -1) = -1
					then (	case when @mwRoomsExtraPlaces <> '0' 
							then isnull(rm_nplacesex, 0)
							else isnull(ac_nmenexbed, 0)
							end)
					else (	case when @findByAdultChild = 1 -- искать по детям
							then isnull(AC_NCHMAIN, 0) + isnull(AC_NCHEXTRA, 0)
							-- искать по дополнительным местам
							else isnull(AC_NADEXTRA, 0) + isnull(AC_NCHEXTRA, 0)
							end)
					end),
			hr_main, 
			to_datevalid, 
			to_datecreated, 
			td_date,
			ti_totaldays,
			ti_nights,
			to_cnkey, 
			isnull(ti_ctkeyfrom, 0),
			ti_apkeyfrom,
			ti_ctkeyto,
			ti_apkeyto, 
			chb.ch_citykeyfrom,
			chb.CH_CITYKEYTO,
			to_key, 
			tl_tip,
			tl_key, 
			ti_key, 
			tp_key,
			tp_gross, 
			ti_firsthdkey, 
			ti_hdpartnerkey,
			hd_rskey, 
			hd_ctkey, 
			hd_stars, 
			ti_firstpnkey,
			ti_firsthrkey, 
			hr_rmkey, 
			hr_rckey, 
			hr_ackey,
			ac_agefrom, 
			ac_ageto, 
			ac_agefrom2,
			ac_ageto2, 
			hd_name, 
			substring(tl_nameweb,1,128), 
			pn_name, 
			pn_code, 
			rm_name, 
			rm_code,
			rc_name, 
			rc_code, 
			ac_name, 
			ac_code, 
			rs_name,
			ct_name, 
			rm_order, 
			rc_order, 
			ac_order,
			to_rate,
			tl_webhttp,
			hd_http, 
			@forceEnable,
			ti_chkey,
			ti_chbackkey,
			ti_hdday,
			ti_hdnights,
			ti_chday,
			ti_chpkkey,
			ti_chprkey,
			ti_chbackday,
			ti_chbackpkkey,
			ti_chbackprkey,
			ti_hotelkeys,
			ti_hotelroomkeys,
			ti_roomtypekeys,
			ti_hotelstars,
			ti_pansionkeys,
			ti_hoteldays,
			tt_charterto,
			tt_charterback,
			to_pricefor,
			tl_attribute,
			tt_tourhotels,
			tt_directFlightAttribute,
			tt_backFlightAttribute
		from tp_tours with(nolock)
			inner join turList with(nolock) on to_trkey = tl_key
			inner join #tp_lists with(nolock) on ti_tokey = to_key
			inner join tp_prices with(nolock) on tp_tikey = ti_key
			inner join tp_turdates with(nolock) on (td_tokey = to_key and td_date between tp_datebegin and tp_dateend)
			inner join hoteldictionary with(nolock) on ti_firsthdkey = hd_key
			inner join hotelrooms with(nolock) on ti_firsthrkey = hr_key
			inner join pansion with(nolock) on ti_firstpnkey = pn_key
			inner join rooms with(nolock) on hr_rmkey = rm_key
			inner join roomscategory with(nolock) on hr_rckey = rc_key
			inner join accmdmentype with(nolock) on hr_ackey = ac_key
			inner join citydictionary with(nolock) on hd_ctkey = ct_key
			left outer join charter as chb with(nolock) on ti_chbackkey = chb.ch_key
			left outer join resorts with(nolock) on hd_rskey = rs_key
			inner join #tempTourInfo on tt_tikey = ti_key
		where
			to_key = @tokey and TP_CalculatingKey = @calcKey
	end
	else
	begin
		insert into #tempPriceTable (
			[pt_mainplaces],
			[pt_addplaces],
			[pt_main],
			[pt_tourvalid],
			[pt_tourcreated],
			[pt_tourdate],
			[pt_days],
			[pt_nights],
			[pt_cnkey],
			[pt_ctkeyfrom],
			[pt_apkeyfrom],
			[pt_ctkeyto],
			[pt_apkeyto],
			[pt_ctkeybackfrom],
			[pt_ctkeybackto],
			[pt_tourkey],
			[pt_tourtype],
			[pt_tlkey],
			[pt_pricelistkey],
			[pt_pricekey],
			[pt_price],
			[pt_hdkey],
			[pt_hdpartnerkey],
			[pt_rskey],
			[pt_ctkey],
			[pt_hdstars],
			[pt_pnkey],
			[pt_hrkey],
			[pt_rmkey],
			[pt_rckey],
			[pt_ackey],
			[pt_childagefrom],
			[pt_childageto],
			[pt_childagefrom2],
			[pt_childageto2],
			[pt_hdname],
			[pt_tourname],
			[pt_pnname],
			[pt_pncode],
			[pt_rmname],
			[pt_rmcode],
			[pt_rcname],
			[pt_rccode],
			[pt_acname],
			[pt_accode],
			[pt_rsname],
			[pt_ctname],
			[pt_rmorder],
			[pt_rcorder],
			[pt_acorder],
			[pt_rate],
			[pt_toururl],
			[pt_hotelurl],
			[pt_isenabled],
			[pt_chkey],
			[pt_chbackkey],
			[pt_hdday],
			[pt_hdnights],
			[pt_chday],
			[pt_chpkkey],
			[pt_chprkey],
			[pt_chbackday],
			[pt_chbackpkkey],
			[pt_chbackprkey],
			pt_hotelkeys,
			pt_hotelroomkeys,
			pt_roomtypekeys,
			pt_hotelstars,
			pt_pansionkeys,
			pt_hotelnights,
			pt_chdirectkeys,
			pt_chbackkeys,
			[pt_topricefor],
			pt_tlattribute,
			pt_hddetails,
			pt_directFlightAttribute,
			pt_backFlightAttribute
		)
		select
				(	case when @mwAccomodationPlaces = '0'
					then isnull(rm_nplaces, 0)
					else (	case when @findByAdultChild = 1 -- искать по взрослым
							then isnull(AC_NADMAIN, 0) + isnull(AC_NADEXTRA,0)
							-- искать по основным
							else isnull(AC_NADMAIN, 0) + isnull(AC_NCHMAIN, 0)
							end)
					end),
				(	case when isnull(ac_nmenexbed, -1) = -1
					then (	case when @mwRoomsExtraPlaces <> '0' 
							then isnull(rm_nplacesex, 0)
							else isnull(ac_nmenexbed, 0)
							end)
					else (	case when @findByAdultChild = 1 -- искать по детям
							then isnull(AC_NCHMAIN, 0) + isnull(AC_NCHEXTRA, 0)
							-- искать по дополнительным местам
							else isnull(AC_NADEXTRA, 0) + isnull(AC_NCHEXTRA, 0)
							end)
					end),
			hr_main, 
			to_datevalid, 
			to_datecreated, 
			td_date,
			ti_totaldays,
			ti_nights,
			to_cnkey, 
			isnull(ti_ctkeyfrom, 0),
			ti_apkeyfrom,
			ti_ctkeyto,
			ti_apkeyto, 
			chb.ch_citykeyfrom, 
			chb.CH_CITYKEYTO,
			to_key, 
			tl_tip,
			tl_key, 
			ti_key, 
			tp_key,
			tp_gross, 
			ti_firsthdkey, 
			ti_hdpartnerkey,
			hd_rskey, 
			hd_ctkey, 
			hd_stars, 
			ti_firstpnkey,
			ti_firsthrkey, 
			hr_rmkey, 
			hr_rckey, 
			hr_ackey,
			ac_agefrom, 
			ac_ageto, 
			ac_agefrom2,
			ac_ageto2, 
			hd_name, 
			substring(tl_nameweb,1,128), 
			pn_name, 
			pn_code, 
			rm_name, 
			rm_code,
			rc_name, 
			rc_code, 
			ac_name, 
			ac_code, 
			rs_name,
			ct_name, 
			rm_order, 
			rc_order, 
			ac_order,
			to_rate,
			tl_webhttp,
			hd_http, 
			@forceEnable,
			ti_chkey,
			ti_chbackkey,
			ti_hdday,
			ti_hdnights,
			ti_chday,
			ti_chpkkey,
			ti_chprkey,
			ti_chbackday,
			ti_chbackpkkey,
			ti_chbackprkey,
			ti_hotelkeys,
			ti_hotelroomkeys,
			ti_roomtypekeys,
			ti_hotelstars,
			ti_pansionkeys,
			ti_hoteldays,
			tt_charterto,
			tt_charterback,
			to_pricefor,
			tl_attribute,
			tt_tourhotels,
			tt_directFlightAttribute,
			tt_backFlightAttribute
		from tp_tours with(nolock)
			inner join turList with(nolock) on to_trkey = tl_key
			inner join #tp_lists with(nolock) on ti_tokey = to_key
			inner join tp_prices with(nolock) on tp_tikey = ti_key
			inner join tp_turdates with(nolock) on (td_tokey = to_key and td_date between tp_datebegin and tp_dateend)
			inner join hoteldictionary with(nolock) on ti_firsthdkey = hd_key
			inner join hotelrooms with(nolock) on ti_firsthrkey = hr_key
			inner join pansion with(nolock) on ti_firstpnkey = pn_key
			inner join rooms with(nolock) on hr_rmkey = rm_key
			inner join roomscategory with(nolock) on hr_rckey = rc_key
			inner join accmdmentype with(nolock) on hr_ackey = ac_key
			inner join citydictionary with(nolock) on hd_ctkey = ct_key
			left outer join charter as chb with(nolock) on ti_chbackkey = chb.ch_key
			left outer join resorts with(nolock) on hd_rskey = rs_key
			inner join #tempTourInfo on tt_tikey = ti_key
		where
			to_key = @tokey and TP_TOKey = @tokey
	end	

	--чтобы не перевыставлялись удаленные цены при выставлении тура в он-лайн
	--update #tempPriceTable set pt_isenabled = 0 where exists (select 1 from mwdeleted with (nolock) where del_key = pt_pricekey)

	update dbo.TP_Tours set TO_Progress = 80 where TO_Key = @tokey

	if dbo.mwReplIsPublisher() <= 0
	begin
		insert into dbo.mwPriceDurations (
			sd_tourkey,
			sd_tlkey,
			sd_days,
			sd_nights,
			sd_hdnights
		)
		select distinct
			ti_tokey,
			to_trkey,
			ti_totaldays,
			ti_nights,
			ti_hoteldays
		from #tp_lists with(nolock) inner join tp_tours with(nolock) on ti_tokey = to_key

		-- Даты в поисковой таблице ставим как в таблице туров - чтобы не было двоений MEG00021274
		update mwspodatatable 
		set sd_tourcreated = to_datecreated 
		from tp_tours with(nolock)
		where sd_tourkey = to_key 		
			and to_key = @tokey
			and sd_tourcreated != to_datecreated 

		set @counter = -1
		set @deleteCount = 50000
		set @params = '@counterOut int output'

		-- Переписываем данные из временной таблицы и уничтожаем ее
		if @mwSearchType = 0
		begin
			while(@counter <> 0)
			begin
				if (@calcKey is not null)
					set @sql = 'delete top (' + ltrim(STR(@deleteCount)) +  ') from mwPriceDataTable where pt_pricekey in (select tp_key from tp_prices with(nolock) where TP_CalculatingKey = ' + cast(@calcKey as nvarchar(20)) + '); set @counterOut = @@ROWCOUNT'
				else
					set @sql = 'delete top(' + ltrim(STR(@deleteCount)) + ') from mwPriceDataTable where pt_tourkey = ' + cast(@tokey as nvarchar(20)) + ';set @counterOut = @@ROWCOUNT'
				EXECUTE sp_executesql @sql, @params, @counterOut = @counter output
			end
		
			exec dbo.mwFillPriceTable '#tempPriceTable', 0, 0
		end
		else
		begin			
			declare cur cursor fast_forward for select distinct thd_cnkey, isnull(thd_ctkeyfrom, 0) from #tmpHotelData
			open cur
			fetch next from cur into @countryKey, @cityFromKey
			while @@fetch_status = 0
			begin
				exec dbo.mwCreateNewPriceTable @countryKey, @cityFromKey

				set @counter = -1
				set @params = '@counterOut int output'
				while(@counter <> 0)
				begin
					if (@calcKey is not null)
						set @sql = 'delete top (' + ltrim(rtrim(str(@deleteCount)))  + ') from ' + dbo.mwGetPriceTableName(@countryKey, @cityFromKey) + ' where pt_pricekey in (select tp_key from tp_prices with(nolock) where TP_CalculatingKey = ' + cast(@calcKey as nvarchar(20)) + '); set @counterOut = @@ROWCOUNT'
					else
						set @sql = 'delete top (' + ltrim(rtrim(str(@deleteCount))) + ') from ' + dbo.mwGetPriceTableName(@countryKey, @cityFromKey) + ' where pt_tourkey = ' + cast(@tokey as nvarchar(20)) + '; set @counterOut = @@ROWCOUNT'
					EXECUTE sp_executesql @sql, @params, @counterOut = @counter output
				end

				set @counter = -1
				set @params = '@counterOut int output'
				while(@counter <> 0)
				begin
					if (@calcKey is not null)
						set @sql = 'delete top (' + ltrim(rtrim(str(@deleteCount)))  + ') from mwPriceDataTable where pt_pricekey in (select tp_key from tp_prices with(nolock) where TP_CalculatingKey = ' + cast(@calcKey as nvarchar(20)) + '); set @counterOut = @@ROWCOUNT'
					else
						set @sql = 'delete top (' + ltrim(rtrim(str(@deleteCount))) + ') from mwPriceDataTable where pt_tourkey = ' + cast(@tokey as nvarchar(20)) + '; set @counterOut = @@ROWCOUNT'
					EXECUTE sp_executesql @sql, @params, @counterOut = @counter output
				end

				exec dbo.mwFillPriceTable '#tempPriceTable', @countryKey, @cityFromKey

				--exec dbo.mwCreatePriceTableIndexes @countryKey, @cityFromKey
				fetch next from cur into @countryKey, @cityFromKey
			end		
			close cur
			deallocate cur
		end
	end
	
	if dbo.mwReplIsPublisher() <= 0
	begin

		update dbo.TP_Tours set TO_Progress = 90 where TO_Key = @tokey

		insert into dbo.mwPriceHotels (
			sd_tourkey,
			sd_mainhdkey,
			sd_mainpnkey,
			sd_hdkey,
			sd_hdstars,
			sd_hdctkey,
			sd_hdrskey,
			sd_hrkey,
			sd_rmkey,
			sd_rckey,
			sd_ackey,
			sd_pnkey,
			sd_hdorder)
		select distinct 
			thd_tourkey, 
			thd_firsthdkey, 
			thd_firstpnkey,
			thd_hdkey, 
			thd_hdstars, 
			thd_ctkey, 
			thd_rskey, 
			thd_hrkey, 
			thd_rmkey, 
			thd_rckey, 
			thd_ackey, 
			thd_pnkey,
			thd_hdorder
		from #tmpHotelData

		-- информация об отелях
		insert into mwSpoDataTable (
			sd_tourkey, 
			sd_cnkey, 
			sd_hdkey, 
			sd_hdstars, 
			sd_ctkey, 
			sd_rskey, 
			sd_ctkeyfrom, 
			sd_ctkeyto, 
			sd_tlkey, 
			sd_isenabled, 
			sd_tourcreated,
			sd_main,
			sd_pnkey,
			sd_tourtype,
			sd_cnname,
			sd_tourname,
			sd_hdname,
			sd_ctname,
			sd_rsname,
			sd_ctfromname,
			sd_cttoname,
			sd_tourtypename,
			sd_pncode,
			sd_hotelkeys,
			sd_pansionkeys,
			sd_tourvalid,

			sd_hotelurl,
			sd_hdprkey
		) 
		select distinct 
			thd_tourkey, 
			thd_cnkey, 
			thd_hdkey, 
			thd_hdstars, 
			thd_ctkey, 
			thd_rskey, 
			thd_ctkeyfrom, 
			thd_ctkeyto, 
			thd_tlkey, 
			thd_isenabled, 
			thd_tourcreated,
			thd_hdmain,
			thd_pnkey,
			thd_tourtype,
			thd_cnname,
			thd_tourname,
			thd_hdname,
			thd_ctname,
			thd_rsname,
			thd_ctfromname,
			thd_cttoname,
			thd_tourtypename,
			thd_pncode,
			thd_hotelkeys,
			thd_pansionkeys,
			thd_tourvalid,
			thd_hotelurl,
			thd_hdpartnerkey
		from #tmpHotelData 
		where thd_hdmain > 0

		update mwPriceHotels set ph_sdkey = mwsdt.sd_key
			from mwSpoDataTable mwsdt with(nolock)
			where mwsdt.sd_tourkey = mwPriceHotels.sd_tourkey and mwsdt.sd_hdkey = mwPriceHotels.sd_mainhdkey
				and mwsdt.sd_tourkey = @tokey
				and mwPriceHotels.sd_tourkey = @tokey

		-- Указываем на необходимость обновления в таблице минимальных цен отеля
		update mwHotelDetails 
			set htd_needupdate = 1
			where htd_hdkey in (select thd_hdkey from #tmpHotelData)
			
	end
	
	if dbo.mwReplIsSubscriber() > 0
	begin
		while 1=1
		begin
			delete top (10000) from TP_Prices where tp_tokey = @tokey
			if @@rowcount = 0
				break
		end
	
		while 1=1
		begin
			delete top (10000) from TP_ServiceLists where tl_tokey = @tokey
			if @@rowcount = 0
				break
		end
		
		while 1=1
		begin
			delete top (10000) from TP_Services where ts_tokey = @tokey
			if @@rowcount = 0
				break
		end
		
		while 1=1
		begin
			delete top (10000) from TP_Lists where ti_tokey = @tokey
			if @@rowcount = 0
				break
		end
		-- don't delete from TP_Tours	
	end
	else
	begin
		update tp_lists
		set
			ti_firsthdkey = ti.ti_firsthdkey,
			ti_lasthotelday = ti.ti_lasthotelday,			
			ti_nights = ti.ti_nights,
			ti_hotelkeys = ti.ti_hotelkeys,
			ti_hotelroomkeys = ti.ti_hotelroomkeys,
			ti_hoteldays = ti.ti_hoteldays,
			ti_hotelstars = ti.ti_hotelstars,
			ti_pansionkeys = ti.ti_pansionkeys,
			ti_hdpartnerkey = ti.ti_hdpartnerkey,
			ti_firsthotelpartnerkey = ti.ti_firsthotelpartnerkey,
			ti_hdday = ti.ti_hdday,
			ti_hdnights = ti.ti_hdnights,
			ti_chkey = ti.ti_chkey,
			ti_chday = ti.ti_chday,
			ti_chpkkey = ti.ti_chpkkey,
			ti_chprkey = ti.ti_chprkey,
			ti_ctkeyfrom = ti.ti_ctkeyfrom,
			ti_chbackkey = ti.ti_chbackkey,
			ti_chbackday = ti.ti_chbackday,
			ti_chbackpkkey = ti.ti_chbackpkkey,
			ti_chbackprkey = ti.ti_chbackprkey,
			ti_ctkeyto = ti.ti_ctkeyto,
			ti_apkeyfrom = ti.ti_apkeyfrom,
			ti_apkeyto = ti.ti_apkeyto,
			ti_firstctkey = ti.ti_firstctkey,
			ti_firstrskey = ti.ti_firstrskey,
			ti_firsthdstars = ti.ti_firsthdstars
		from #tp_lists ti
		where
			tp_lists.TI_Key = ti.TI_Key
			and
			(
				isnull(tp_lists.ti_firsthdkey, 0) <> isnull(ti.ti_firsthdkey , 0)
				or isnull(tp_lists.ti_lasthotelday, 0) <> isnull(ti.ti_lasthotelday, 0)
				or isnull(tp_lists.ti_nights, 0) <> isnull(ti.ti_nights, 0)
				or isnull(tp_lists.ti_hotelkeys, 0) <> isnull(ti.ti_hotelkeys, 0)
				or isnull(tp_lists.ti_hotelroomkeys, 0) <> isnull(ti.ti_hotelroomkeys, 0)
				or isnull(tp_lists.ti_hoteldays, 0) <> isnull(ti.ti_hoteldays, 0)
				or isnull(tp_lists.ti_hotelstars, 0) <> isnull(ti.ti_hotelstars, 0)
				or isnull(tp_lists.ti_pansionkeys, 0) <> isnull(ti.ti_pansionkeys, 0)
				or isnull(tp_lists.ti_hdpartnerkey, 0) <> isnull(ti.ti_hdpartnerkey, 0)
				or isnull(tp_lists.ti_firsthotelpartnerkey, 0) <> isnull(ti.ti_firsthotelpartnerkey, 0)
				or isnull(tp_lists.ti_hdday, 0) <> isnull(ti.ti_hdday, 0)
				or isnull(tp_lists.ti_hdnights, 0) <> isnull(ti.ti_hdnights, 0)
				or isnull(tp_lists.ti_chkey, 0) <> isnull(ti.ti_chkey, 0)
				or isnull(tp_lists.ti_chday, 0) <> isnull(ti.ti_chday, 0)
				or isnull(tp_lists.ti_chpkkey, 0) <> isnull(ti.ti_chpkkey, 0)
				or isnull(tp_lists.ti_chprkey, 0) <> isnull(ti.ti_chprkey, 0)
				or isnull(tp_lists.ti_ctkeyfrom, 0) <> isnull(ti.ti_ctkeyfrom, 0)
				or isnull(tp_lists.ti_chbackkey, 0) <> isnull(ti.ti_chbackkey, 0)
				or isnull(tp_lists.ti_chbackday, 0) <> isnull(ti.ti_chbackday, 0)
				or isnull(tp_lists.ti_chbackpkkey, 0) <> isnull(ti.ti_chbackpkkey, 0)
				or isnull(tp_lists.ti_chbackprkey, 0) <> isnull(ti.ti_chbackprkey, 0)
				or isnull(tp_lists.ti_ctkeyto, 0) <> isnull(ti.ti_ctkeyto, 0)
				or isnull(tp_lists.ti_apkeyfrom, 0) <> isnull(ti.ti_apkeyfrom, 0)
				or isnull(tp_lists.ti_apkeyto, 0) <> isnull(ti.ti_apkeyto, 0)
				or isnull(tp_lists.ti_firstctkey, 0) <> isnull(ti.ti_firstctkey, 0)
				or isnull(tp_lists.ti_firstrskey, 0) <> isnull(ti.ti_firstrskey, 0)
				or isnull(tp_lists.ti_firsthdstars, 0) <> isnull(ti.ti_firsthdstars, 0)
			)
	end

	if(@forceEnable > 0 and @calcKey is null)
	begin
		exec mwEnablePriceTourNewSinglePrice @tokey, '#tempPriceTable'

		update tp_tours
		set to_isenabled = 1
		where to_key = @tokey
	end

	drop table #tempPriceTable

	update dbo.TP_Tours
	set TO_Update = 0,
		TO_Progress = 100,
		TO_DateCreated = GetDate()
	where
		TO_Key = @tokey

	if dbo.mwReplIsSubscriber() <= 0
	begin
		update dbo.TP_Tours 
		set TO_UpdateTime = GetDate()
		where
			TO_Key = @tokey
	end

	EXECUTE mwFillPriceListDetails @tokey

if dbo.mwReplIsSubscriber() > 0
	begin
		delete 
		from tp_turdates 
		where td_tokey = @tokey
		and not exists (select 1 from dbo.tp_prices where tp_tokey = td_tokey and tp_datebegin = td_date)
	end

	------------------------------------------------------------
	--заполнение колонок mwSPODataTable для поиска по отелям----

	create table #hdKeys (xKey int identity(1,1), xHdKey int, xCnKey int, xCtKey int)

	declare @key int, @key_big bigint, @hdkeys varchar(256), @cnKeys varchar(256), @ctKeys varchar(256)
	declare directCursor cursor local fast_forward for
	select distinct sd_hotelkeys
	from mwSPODataTable 
	where charindex(',', sd_hotelkeys) > 0 
	and sd_cnkeys is null

	open directCursor
	fetch directCursor into @hdkeys
	while (@@FETCH_STATUS = 0)
	begin

		truncate table #hdKeys
	
		insert into #hdKeys (xHdKey, xCnKey, xCtKey)
		select xt_key, HD_CNKEY, HD_CTKEY
		from ParseKeys(@hdkeys)
		join HotelDictionary on xt_key = HD_KEY

		set @CnKeys = ''
		set @CtKeys = ''
		select @CnKeys = @CnKeys + '|' + ltrim(str(xCnKey)) + '|,', @CtKeys = @CtKeys + '|' + ltrim(str(xCtKey)) + '|,'
		from #hdKeys

		if(len(@CnKeys) > 0)
			set @CnKeys = substring(@CnKeys, 1, len(@CnKeys) - 1)
		if(len(@CtKeys) > 0)
			set @CtKeys = substring(@CtKeys, 1, len(@CtKeys) - 1)

		update mwSPODataTable set sd_cnkeys = @CnKeys, sd_ctkeys = @CtKeys where sd_hotelkeys = @hdkeys and sd_cnkeys is null

		fetch directCursor into @hdkeys
	end
	close directCursor
	deallocate directCursor

	declare directCursor cursor local fast_forward for
	select distinct pt_hotelkeys
	from mwPriceDataTable
	where charindex(',', pt_hotelkeys) > 0 
	and pt_cnkeys is null

	open directCursor
	fetch directCursor into @hdkeys
	while (@@FETCH_STATUS = 0)
	begin

		truncate table #hdKeys
	
		insert into #hdKeys (xHdKey, xCnKey, xCtKey)
		select xt_key, HD_CNKEY, HD_CTKEY
		from ParseKeys(@hdkeys)
		join HotelDictionary on xt_key = HD_KEY

		set @CnKeys = ''
		set @CtKeys = ''
		select @CnKeys = @CnKeys + '|' + ltrim(str(xCnKey)) + '|,', @CtKeys = @CtKeys + '|' + ltrim(str(xCtKey)) + '|,'
		from #hdKeys

		if(len(@CnKeys) > 0)
			set @CnKeys = substring(@CnKeys, 1, len(@CnKeys) - 1)
		if(len(@CtKeys) > 0)
			set @CtKeys = substring(@CtKeys, 1, len(@CtKeys) - 1)

		update mwPriceDataTable set pt_cnkeys = @CnKeys, pt_ctkeys = @CtKeys where pt_hotelkeys = @hdkeys and pt_cnkeys is null

		fetch directCursor into @hdkeys
	end
	close directCursor
	deallocate directCursor

	drop table #hdKeys
	------------------------------------------------------------

end
GO

grant exec on [dbo].[FillMasterWebSearchFields] to public
GO