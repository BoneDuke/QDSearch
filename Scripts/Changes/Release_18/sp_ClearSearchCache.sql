CREATE proc [dbo].[ClearSearchCache] @toKey int = null, @cnKey int = null, @ctKeyFrom int = null
as
begin
	if (@toKey is null)
	begin
		select top 1 @toKey = to_key
		from tp_tours
		join tbl_TurList on TL_Key = to_trkey
		where to_isenabled = 1
		and to_datevalid > dateadd(day, -1, getdate())
		and to_cnkey = @cnKey
		and tl_ctdeparturekey = @ctKeyFrom
	end

	insert into mwReplQueue (rq_tokey, rq_mode, rq_state, rq_crdate, rq_startdate, rq_enddate)
	values (@toKey, 0, 5, getdate(), getdate(), getdate())
end
GO

grant exec on ClearSearchCache to public
GO