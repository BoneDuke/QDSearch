if exists(select 1 from sys.sysobjects where xtype = 'P' and name = 'SPGetPansions')
begin
	drop procedure [dbo].[SPGetPansions]
end
GO

-- =============================================
-- Author:		
-- Create date: 2013-12-01
-- Description:	Возвращает список питаний по опред. фильтру
-- =============================================
CREATE procedure [dbo].[SPGetPansions] 
	@filter1 varchar(max),
	@filter2 varchar(max)
as
begin
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	declare @query varchar(max)
	set @query = 'select DISTINCT PN_KEY as [Key], PN_NAME as Name
from mwPriceHotels as hot
join Pansion ON hot.sd_pnkey = PN_KEY
where hot.sd_tourkey in (select sd_tourkey
from mwPriceDurations
INNER JOIN TP_TurDates ON sd_tourkey = TD_TOKey where ' + @filter2 + ') ' +
@filter1 + '
ORDER BY PN_Name'

	exec (@query)
	
end
GO

grant execute on [dbo].[SPGetPansions]  to public
GO