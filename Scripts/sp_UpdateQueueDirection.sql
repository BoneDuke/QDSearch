if exists (select 1 from sys.objects where name like '%UpdateQueueDirection%' and type = 'p')
begin
	DROP PROCEDURE [dbo].[UpdateQueueDirection]
end
GO

create PROCEDURE [dbo].[UpdateQueueDirection] (@rqId int, @toKey int) 
AS
begin
	create table #tourDirections (xCtKey int, xCnKey int)

	insert into #tourDirections (xCtKey, xCnKey)
	select distinct sd_ctkeyfrom, sd_cnkey
	from mwSpoDataTable
	where sd_tourkey = @toKey
	and sd_cnkeys is null

	declare @ctKeyFrom int, @cnKeys varchar(256)
	declare cnCursor cursor local fast_forward for
	select distinct sd_ctkeyfrom, sd_cnkeys
	from mwSPODataTable 
	where sd_tourkey = @toKey
	and sd_cnkeys is not null

	open cnCursor
	fetch cnCursor into @ctKeyFrom, @cnKeys
	while (@@FETCH_STATUS = 0)
	begin

		insert into #tourDirections (xCtKey, xCnKey)
		select distinct @ctKeyFrom, xt_key
		from ParseKeys(REPLACE (@cnKeys, '|', ''))
		where not exists (select 1 from #tourDirections where xCtKey = @ctKeyFrom and xCnKey = xt_key)

		fetch cnCursor into @ctKeyFrom, @cnKeys
	end
	close cnCursor
	deallocate cnCursor

	declare @count int
	select @count = count(1) from #tourDirections

	if (@count) > 1
	begin
		insert into mwReplQueue (rq_crdate, rq_mode, rq_tokey, rq_state, rq_priority, rq_startdate, rq_enddate, rq_cnkey, rq_ctkeyfrom)
		select getdate(), 0, -1, 5, 500, getdate(), getdate(), xCnKey, xCtKey
		from #tourDirections
	end
	else if (@count = 1)
	begin
		update mwReplQueue 
		set rq_cnkey = xCnKey,
		rq_ctkeyfrom = xCtKey
		from #tourDirections
		where rq_id = @rqId
	end

end
GO

grant exec on [dbo].[UpdateQueueDirection] to public
GO