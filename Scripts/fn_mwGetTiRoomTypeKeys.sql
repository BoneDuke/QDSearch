
CREATE function [dbo].[mwGetTiRoomTypeKeys] (@tikey int) returns varchar(256) 	
as
begin
declare @res varchar(100)
set @res = ''
select 
	@res = @res + isnull(ltrim(str(HR_RMKEY)),'') + ','
from 
	tp_services with(nolock) 
	inner join tp_servicelists with(nolock) on tl_tskey = ts_key
	inner join HotelRooms with(nolock) on ts_subcode1 = hr_key
where
	ts_svkey = 3 and
	tl_tikey = @tikey
order by
	ts_day

if(len(@res) > 0)
	set @res = substring(@res, 1, len(@res) - 1)

return @res
end