if exists (select 1 from sys.objects where name like '%mwReplProcessQueueDivide%' and type = 'p')
begin
	DROP PROCEDURE [dbo].[mwReplProcessQueueDivide]
end
GO

create procedure [dbo].[mwReplProcessQueueDivide] 
(
	@jobId smallint = null,
	@countryKeysToProcess ListIntValue readonly,		-- ключи стран, которые должны обрабатываться (берутся только переданные страны)
	@countryKeysToNotProcess ListIntValue readonly	-- ключи стран, которые не должны обрабатываться (берутся все, кроме переданных стран)
	-- если переданы одновременно @countryKeysToProcess и @countryKeysToNotProcess, то произойдет ошибка
)
as
begin
	--<VERSION>2009.2.20</VERSION>
	--<DATE>2014-02-14</DATE>
	if dbo.mwReplIsSubscriber() <= 0
		return

	--if exists(select top 1 1 from mwReplQueue with(nolock) where rq_state = 4 and DATEDIFF(MINUTE, rq_startdate, GETDATE()) > 10 and rq_priority > 0)
	--begin
	--	delete from mwReplQueue where rq_tokey not in (select to_key from TP_Tours) and rq_mode <> 4 and (rq_startdate is null or rq_state = 4)
		
	--	update mwReplQueue set rq_state = 1, rq_startdate = null, rq_enddate = null, rq_priority = rq_priority - 1
	--	where rq_state = 4 
	--	and DATEDIFF(MINUTE, rq_startdate, GETDATE()) > 10
	--	and rq_priority > 0

	--end

	if exists (select top 1 1 from @countryKeysToProcess)
		and exists (select top 1 1 from @countryKeysToNotProcess)
	begin
		RAISERROR('must pass only one of @countryKeysToProcess and @countryKeysToNotProcess or neither of them', 16, 1)
		return
	end

	-- обновляем инфу о стране и городе вылета по туру
	if exists(select 1 from mwReplQueue with(nolock) where rq_state = 1 and rq_cnkey is null)
	begin
		update mwReplQueue
		set rq_cnkey = TO_CNKey,
		rq_ctkeyfrom = TL_CTDepartureKey
		from tp_tours
		join tbl_TurList on tl_key = to_trkey
		where to_key = rq_tokey
		and rq_cnkey is null
		and rq_state = 1
	end
		
	if (@jobId is null)
		set @jobId = @@SPID
		
	-- такое может происходить, только если произошла аварийная остановка джоба и его повторный запуск
	-- апдейтим таблицу направлений и таблицу очереди
	if exists(select 1 from mwReplDirections where RD_IsUsed = @jobId)
	begin
		update mwReplQueue 
		set rq_state = 4 
		from mwReplDirections
		where RD_CNKey = rq_cnkey
		and RD_CTKeyFrom = rq_ctkeyfrom
		and rq_state = 3
		and RD_IsUsed = @jobId
		
		update mwReplDirections set RD_IsUsed = 0 where RD_IsUsed = @jobId		
	end
		
	declare @mwSearchType int
	declare @cnKey int, @ctKey int
	select @mwSearchType = isnull(SS_ParmValue, 1) from dbo.systemsettings with(nolock) 
	where SS_ParmName = 'MWDivideByCountry'

	declare @rqId int
	declare @rqMode int
	declare @rqToKey int
	declare @rqCalculatingKey int
	declare @rqOverwritePrices bit	

	declare @selectedDirections table(CNKey int, CTKey int)
	declare @currentQueue table(xrq_id int, xrq_mode int, xrq_tokey int, xrq_CalculatingKey int, xRQ_OverwritePrices bit, xrq_state int, xrq_enddate datetime)

	declare @directionsCount as smallint
	select @directionsCount = count(*) from @countryKeysToProcess

	if @directionsCount = 0
		select @directionsCount = count(*) from @countryKeysToProcess

	-- select directions
	if exists (select top 1 1 from @countryKeysToProcess)
	begin
		insert into @selectedDirections
		select isnull(rq_cnkey, 0), isnull(rq_ctkeyfrom, 0)
		from mwReplQueue with(nolock)
		join mwReplDirections with(nolock) on rd_cnkey = isnull(rq_cnkey, 0) and rd_ctkeyfrom = isnull(rq_ctkeyfrom, 0)
		where rd_isUsed = 0
		and (rq_state = 1 or rq_state = 2)
		and rq_mode <= 5
		and isnull(rq_cnkey, 0) in (select value from @countryKeysToProcess)
		order by rq_priority desc, rq_crdate
	end
	else if exists (select top 1 1 from @countryKeysToNotProcess)
	begin
		insert into @selectedDirections
		select isnull(rq_cnkey, 0), isnull(rq_ctkeyfrom, 0)
		from mwReplQueue with(nolock)
		join mwReplDirections with(nolock) on rd_cnkey = isnull(rq_cnkey, 0) and rd_ctkeyfrom = isnull(rq_ctkeyfrom, 0)
		where rd_isUsed = 0
		and (rq_state = 1 or rq_state = 2)
		and rq_mode <= 5
		and isnull(rq_cnkey, 0) not in (select value from @countryKeysToNotProcess)
		order by rq_priority desc, rq_crdate
	end
	else
	begin
		insert into @selectedDirections
		select top 1 isnull(rq_cnkey, 0), isnull(rq_ctkeyfrom, 0)
		from mwReplQueue with(nolock)
		join mwReplDirections with(nolock) on rd_cnkey = isnull(rq_cnkey, 0) and rd_ctkeyfrom = isnull(rq_ctkeyfrom, 0)
		where rd_isUsed = 0
		and (rq_state = 1 or rq_state = 2)
		and rq_mode <= 5
		order by rq_priority desc, rq_crdate
	end

	update mwReplDirections 
	set RD_IsUsed = @jobId
	where RD_IsUsed = 0 
		and exists (select top 1 1 from @selectedDirections where cnKey = rd_cnkey and ctKey = RD_CTKeyFrom)

	if not exists(select 1 from mwReplDirections where RD_IsUsed = @jobId)
		return
		
	-- select commands by directions
	insert into @currentQueue (xrq_id, xrq_mode, xrq_tokey, xrq_CalculatingKey, xRQ_OverwritePrices)
	select top 1 rq_id, rq_mode, rq_tokey, rq_CalculatingKey, RQ_OverwritePrices
	from mwReplQueue 
	where (rq_state = 1 or rq_state = 2)
	and exists (select top 1 1 from @selectedDirections where cnKey = isnull(rq_cnkey, 0) and ctKey = isnull(rq_ctkeyfrom, 0))
	and rq_mode <= 5
	order by rq_priority desc, rq_crdate
	
	update mwReplQueue set [rq_state] = 3, [rq_startdate] = getdate() where rq_id in (select xrq_id from @currentQueue)
	
	declare queueCursor cursor local fast_forward for
	select xrq_id, xrq_mode, xrq_tokey, xrq_CalculatingKey, xRQ_OverwritePrices
	from @currentQueue
	
	-- process commands
	open queueCursor
	fetch queueCursor into @rqId, @rqMode, @rqToKey, @rqCalculatingKey, @rqOverwritePrices
	
	while (@@FETCH_STATUS = 0)
	begin

		update mwReplQueue set rq_startdate = getdate() where rq_id = @rqId
		
		insert into mwReplQueueHistory([rqh_rqid], [rqh_text])
		select @rqId, 'Command start.'
			
		begin try	
			if (@rqMode = 1)
			begin
				exec FillMasterWebSearchFields @tokey = @rqToKey, @calcKey = @rqCalculatingKey, @overwritePrices = @rqOverwritePrices
				exec SftWebPsDB.dbo.UpdateSearchFilter @rqToKey, 1
			end
			else if (@rqMode = 2)
			begin
				exec FillMasterWebSearchFields @tokey = @rqToKey, @calcKey = @rqCalculatingKey, @overwritePrices = @rqOverwritePrices
				exec SftWebPsDB.dbo.UpdateSearchFilter @rqToKey, 1
			end
			else if (@rqMode = 3)
			begin
				exec mwReplDisablePriceTour @rqToKey, @rqId
				exec SftWebPsDB.dbo.UpdateSearchFilter @rqToKey, 0
			end
			else if (@rqMode = 4)
			begin
				exec mwReplDeletePriceTour @rqToKey, @rqId
				exec SftWebPsDB.dbo.UpdateSearchFilter @rqToKey, 0
			end
			else if (@rqMode = 5)
			begin
				exec mwReplUpdatePriceTourDateValid @rqToKey, @rqId

				declare @dateValid datetime
				select @dateValid = TO_DateValid
				from tp_tours 
				where to_key = @rqToKey
				exec SftWebPsDB.dbo.UpdateSearchFilter @rqToKey, 2, @dateValid
			end
			
			update mwReplQueue set rq_state = 5, rq_enddate = getdate() where rq_id = @rqId
			
			insert into mwReplQueueHistory([rqh_rqid], [rqh_text])
			select @rqId, 'Command complete.'
		
		end try
		begin catch
			update mwReplQueue set rq_state = 4, rq_enddate = getdate() where rq_id = @rqId
			
			declare @errMessage varchar(max)
			set @errMessage = 'Error at ' + isnull(ERROR_PROCEDURE(), '[mwReplProcessQueueDivide]') +' : ' + isnull(ERROR_MESSAGE(), '[msg_not_set]')
			
			insert into mwReplQueueHistory([rqh_rqid], [rqh_text])
			select @rqId, @errMessage
		end catch
		
		fetch queueCursor into @rqId, @rqMode, @rqToKey, @rqCalculatingKey, @rqOverwritePrices
		
	end
	
	close queueCursor
	deallocate queueCursor
	
	update mwReplDirections set rd_isUsed = 0 where rd_isUsed = @jobId
	
end
GO

grant exec on [dbo].[mwReplProcessQueueDivide] to public
GO