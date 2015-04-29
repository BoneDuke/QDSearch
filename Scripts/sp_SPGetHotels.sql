if exists(select 1 from sys.sysobjects where xtype = 'P' and name = 'SPGetHotels')
begin
	drop procedure [dbo].[SPGetHotels]
end
GO

-- =============================================
-- Author:		
-- Create date: 2013-12-01
-- Description:	Возвращает список питаний по опред. фильтру
-- =============================================
CREATE procedure [dbo].[SPGetHotels] 
	@filter1 varchar(max),
	@filter2 varchar(max)
as
begin
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	declare @query varchar(max)
	set @query = 'select HD_Key, HD_Name, RS_Key, ISNULL(RS_NAME, ''), HD_HTTP, HD_Stars
from HotelDictionary
left join Resorts on RS_Key = HD_RSKey
where HD_Key in (select distinct sd_hdkey 
from mwPriceHotels as h
join TP_TurDates on td_tokey = h.sd_tourkey
join mwPriceDurations as d on d.sd_tourkey = td_tokey
where ' + @filter2 + ')' + @filter1

	print (@query)
	exec (@query)
	
end
GO

grant execute on [dbo].[SPGetHotels]  to public
GO