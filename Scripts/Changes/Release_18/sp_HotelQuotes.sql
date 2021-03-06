
ALTER PROCEDURE [dbo].[mwHotelQuotes]
	(
		-- хранимка получает сведения о квотах для отелей
		--<version>2009.2.01</version>
		--<data>2012-11-09</data>
		@Filter varchar(2000),
		@DaysCount int,
		@AgentKey int, 
		@FromDate	datetime,
		@RequestOnRelease smallint,
		@NoPlacesResult int,
		@CheckAgentQuotes smallint,
		@CheckCommonQuotes smallint,
		@ExpiredReleaseResult int
	)
AS
BEGIN

DECLARE @checkQuotesOnWebService as bit, @checkQuotesService as nvarchar(150)
DECLARE @webServiceFailure as bit

-- признак ошибки веб-сервиса
SET @webServiceFailure = 0
-- проверять квоты через веб-сервис
SET @checkQuotesOnWebService = 0
SELECT TOP 1 @checkQuotesOnWebService = ss_parmvalue FROM systemsettings WITH (nolock) WHERE ss_parmname = 'NewSetToQuota'

-- создание временной таблицы
CREATE TABLE #tmp
(
	CityKey int,
	CityName varchar(50) COLLATE Cyrillic_General_BIN,
	HotelKey int,
	HotelName varchar(200) COLLATE Cyrillic_General_BIN,
	HotelHTTP varchar(254),
	RoomKey int,
	RoomName varchar(35) COLLATE Cyrillic_General_BIN,
	RoomCategoryKey int,
	RoomCategoryName varchar(60) COLLATE Cyrillic_General_BIN,
	Quotas varchar(2000),
	HotelRoomsKey int,
	HotelRoomsMain int
)

-- формирование данных
DECLARE	@HotelKey int
DECLARE	@RoomKey int 
DECLARE	@RoomCategoryKey int 
DECLARE @HotelRoomsKey int
DECLARE @HotelRoomsMain int
DECLARE @freePlacesMask int

DECLARE @script VARCHAR(4000)
SET @script = 'SELECT DISTINCT SD_CTKEY, SD_CTNAME, mwSpoDataTable.SD_HDKEY, SD_HDNAME  + '' ('' + ISNULL(SD_RSNAME, SD_CTNAME) + '') '' + mwSpoDataTable.SD_HDSTARS as HotelName,
				ISNULL(HD_HTTP, ''''), SD_RMKEY, RM_NAME, SD_RCKEY, RC_NAME, '''', HR_Key, HR_Main 
	FROM mwPriceHotels with(nolock)
		JOIN mwSpoDataTable with(nolock) ON mwPriceHotels.PH_SDKEY = mwSpoDataTable.SD_KEY
		JOIN Rooms with(nolock) ON SD_RMKEY = RM_KEY		
		JOIN RoomsCategory with(nolock) ON SD_RCKEY = RC_KEY
		JOIN HotelDictionary with(nolock) ON mwSpoDataTable.SD_HDKEY = HD_KEY
		JOIN HotelRooms with(nolock) ON (SD_HRKey = HR_Key)
		WHERE ' + @filter + ' ORDER BY HotelName'

INSERT INTO #tmp EXEC(@script)

-- если стоит флаг проверки через веб-сервис
if @checkQuotesOnWebService = 1
BEGIN TRY
	DECLARE hSql CURSOR 
	FOR 
		SELECT HotelKey, RoomKey, RoomCategoryKey, HotelRoomsKey, HotelRoomsMain FROM #tmp
	FOR UPDATE OF Quotas

	OPEN hSql
	FETCH NEXT FROM hSql INTO @HotelKey, @RoomKey, @RoomCategoryKey, @HotelRoomsKey, @HotelRoomsMain

	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @checkQuotesResult int, @places int, @allPlaces int
		SELECT @checkQuotesResult = result, @places = freePlaces, @allPlaces = allPlaces FROM [dbo].WcfQuotaCheckOneResult(0, 3, @HotelKey, @HotelRoomsKey, @FromDate, @FromDate, -1, @AgentKey, @DaysCount, @HotelRoomsMain, null)
		UPDATE #tmp SET Quotas = '0=' + @places + ':' + @allplaces
			WHERE current of hSql
		FETCH NEXT FROM hSql INTO @HotelKey, @RoomKey, @RoomCategoryKey, @HotelRoomsKey, @HotelRoomsMain
	END
	CLOSE hSql
	DEALLOCATE hSql
END TRY
BEGIN CATCH
	SET @webServiceFailure = 1
	CLOSE hSql
	DEALLOCATE hSql
END CATCH

-- если произошла ошибка или стоит флаг проверки обычным методом
if @checkQuotesOnWebService = 0 or @webServiceFailure = 1
BEGIN
	DECLARE hSql CURSOR 
	FOR 
		SELECT HotelKey, RoomKey, RoomCategoryKey, HotelRoomsKey, HotelRoomsMain FROM #tmp
	FOR UPDATE OF Quotas

	OPEN hSql
	FETCH NEXT FROM hSql INTO @HotelKey, @RoomKey, @RoomCategoryKey, @HotelRoomsKey, @HotelRoomsMain

	WHILE @@FETCH_STATUS = 0
	BEGIN

		UPDATE #tmp SET Quotas = (select top 1 qt_additional from mwCheckQuotesEx(3, @HotelKey, @RoomKey, @RoomCategoryKey, @AgentKey, -1, @FromDate, 1, @DaysCount, @RequestOnRelease, @NoPlacesResult, @CheckAgentQuotes, @CheckCommonQuotes, 0, 0, 0, 0, 0, 0, @ExpiredReleaseResult))
			WHERE current of hSql
		FETCH NEXT FROM hSql INTO @HotelKey, @RoomKey, @RoomCategoryKey, @HotelRoomsKey, @HotelRoomsMain
	END
	CLOSE hSql
	DEALLOCATE hSql
END

CREATE TABLE #tmp2
(
	CityKey int,
	CityName varchar(50) COLLATE Cyrillic_General_BIN,
	HotelKey int,
	HotelName varchar(200) COLLATE Cyrillic_General_BIN,
	HotelHTTP varchar(254),
	RoomKey int,
	RoomName varchar(35) COLLATE Cyrillic_General_BIN,
	RoomCategoryKey int,
	RoomCategoryName varchar(60) COLLATE Cyrillic_General_BIN,
	Quotas varchar(2000)
)

-- исключаем данные, для совместимости с прошлыми версиями
SET @script = 'SELECT DISTINCT CityKey, CityName, HotelKey, HotelName, HotelHTTP, RoomKey, RoomName, RoomCategoryKey, RoomCategoryName, Quotas FROM #tmp'
INSERT INTO #tmp2 EXEC(@script)
SELECT * FROM #tmp2 ORDER BY HotelName

-- удаление временной таблицы
DROP TABLE  #tmp
DROP TABLE  #tmp2

END

