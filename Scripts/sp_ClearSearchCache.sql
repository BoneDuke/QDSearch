
ALTER proc [dbo].[ClearSearchCache] @cnKey int, @ctKeyFrom int
as
begin
	insert into mwReplQueue (rq_tokey, rq_mode, rq_state, rq_crdate, rq_startdate, rq_enddate, rq_cnkey, rq_ctkeyfrom)
	values (-1, 0, 5, getdate(), getdate(), getdate(), @cnKey, @ctKeyFrom)
end
