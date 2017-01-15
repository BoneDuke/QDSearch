--alter table mwSPODataTable add sd_cnkeys varchar(256) null
--alter table mwSPODataTable add sd_ctkeys varchar(256) null
--sp_refreshviewforall 'mwSPOData'

create table #hdKeys (xKey int identity(1,1), xSdKey int, xHdKey int, xCnKey int, xCtKey int, xCnKeys varchar(256), xCtKeys varchar(256))

truncate table #hdKeys
declare @key int, @hdkeys varchar(256), @cnKeys varchar(256), @ctKeys varchar(256)
declare directCursor cursor local fast_forward for
select sd_key, sd_hotelkeys
from mwSPODataTable 
where charindex(',', sd_hotelkeys) > 0 
and sd_cnkeys is null

open directCursor
fetch directCursor into @key, @hdkeys
while (@@FETCH_STATUS = 0)
begin

	truncate table #hdKeys
	
	insert into #hdKeys (xHdKey, xCnKey, xCtKey)
	select xt_key, HD_CNKEY, HD_CTKEY
	from ParseKeys(@hdkeys)
	join HotelDictionary on xt_key = HD_KEY

	set @CnKeys = ''
	set @CtKeys = ''
	select @CnKeys = @CnKeys + '[' + ltrim(str(xCnKey)) + '],', @CtKeys = @CtKeys + '[' + ltrim(str(xCtKey)) + '],'
	from #hdKeys
	if(len(@CnKeys) > 0)
		set @CnKeys = substring(@CnKeys, 1, len(@CnKeys) - 1)
	if(len(@CtKeys) > 0)
		set @CtKeys = substring(@CtKeys, 1, len(@CtKeys) - 1)

	update mwSPODataTable set sd_cnkeys = @CnKeys, sd_ctkeys = @CtKeys where sd_key = @key

	fetch directCursor into @key, @hdkeys
end
close directCursor
deallocate directCursor

drop table #hdKeys