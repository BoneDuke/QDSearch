
ALTER procedure [dbo].[mwReplDisableDeletedPrices]
as
begin
	declare @cnKey int
	declare @ctKeyFrom int
	declare @toKey int

	select top 100000 * into #mwReplDeletedPricesTemp from dbo.mwReplDeletedPricesTemp with(nolock);
	create index x_pricekey on #mwReplDeletedPricesTemp(rdp_pricekey);
	
	delete from dbo.mwReplDeletedPricesTemp
	where exists(select 1 from #mwReplDeletedPricesTemp r where r.rdp_pricekey = mwReplDeletedPricesTemp.rdp_pricekey);
	
	if dbo.mwReplIsPublisher() > 0 
	begin
		declare @sql varchar (500);
		declare @source varchar(200);
		set @source = '';
		
		if exists(select 1 from #mwReplDeletedPricesTemp)
		begin
			
			DECLARE @subscriptionLinkedServer as nvarchar(50)
			DECLARE @subscriptionDatabaseName as nvarchar(100)			

			DECLARE subscriptionsCursor CURSOR FOR
			SELECT linkedServerName, subscriptionDatabaseName
			FROM [dbo].[mwReplGetSubscriptions]()

			BEGIN TRY

				OPEN subscriptionsCursor

				FETCH NEXT FROM subscriptionsCursor INTO @subscriptionLinkedServer, @subscriptionDatabaseName
				
				WHILE @@Fetch_Status = 0
				BEGIN
				
					SET @source = @subscriptionLinkedServer + '.' + @subscriptionDatabaseName
				
					SET @sql = '
					insert into ' + @source + '.dbo.mwReplDeletedPricesTemp (rdp_pricekey, rdp_cnkey, rdp_ctdeparturekey)
					select rdp_pricekey, rdp_cnkey, rdp_ctdeparturekey from #mwReplDeletedPricesTemp';

					EXEC (@sql);
					
					FETCH NEXT FROM subscriptionsCursor INTO @subscriptionLinkedServer, @subscriptionDatabaseName
				
				END

			END TRY
			BEGIN CATCH

				DECLARE @errorMessage as nvarchar(max)
				SET @errorMessage = 'Error in mwReplDisableDeletedPrices: ' + ERROR_MESSAGE()

				INSERT INTO SystemLog (sl_date, sl_message)
				VALUES (getdate(), @errorMessage)
				
				RAISERROR (@errorMessage, 18, 100); 

			END CATCH

			CLOSE subscriptionsCursor
			DEALLOCATE subscriptionsCursor
		end
	end
	else if dbo.mwReplIsSubscriber() > 0
	begin
		if exists(select 1 from #mwReplDeletedPricesTemp)
		begin
			insert into dbo.mwDeleted (del_key)
			select rdp_pricekey from #mwReplDeletedPricesTemp;

			create table #delKeys (xKey int)
			
			if exists(select 1 from SystemSettings where SS_ParmName = 'MWDivideByCountry' and SS_ParmValue = 1)
			begin
				--Используется секционирование ценовых таблиц			
				declare mwPriceDataTableNameCursor cursor for
					select distinct dbo.mwGetPriceTableName(rdp_cnkey, rdp_ctdeparturekey) as ptn_tablename, rdp_cnkey, rdp_ctdeparturekey
					from
						#mwReplDeletedPricesTemp with(nolock);
					
				declare @mwPriceDataTableName varchar(200);
				open mwPriceDataTableNameCursor;
				fetch next from mwPriceDataTableNameCursor into @mwPriceDataTableName, @cnKey, @ctKeyFrom;

				while @@FETCH_STATUS = 0
				begin
					if exists (select * from sys.objects where object_id = OBJECT_ID(N'[dbo].[' + @mwPriceDataTableName + ']') AND type in (N'U'))
					begin
						set @sql='
							update [dbo].[' + @mwPriceDataTableName + ']
							set pt_isenabled = 0
							where exists(select 1 from #mwReplDeletedPricesTemp r where r.rdp_pricekey = pt_pricekey and r.rdp_cnkey = ' + ltrim(str(@cnKey)) + ' and r.rdp_ctdeparturekey = ' + ltrim(str(@ctKeyFrom)) + ')
							and pt_isenabled = 1';
							--print (@sql)
							exec (@sql);

						set @sql = 'insert into #delKeys (xKey) 
							select sd_key
							from mwSpoDataTable
							where sd_isenabled = 1
							and sd_cnkey = ' + ltrim(str(@cnKey)) + ' and sd_ctkeyfrom = ' + ltrim(str(@ctKeyFrom)) + '
							and sd_hdkey not in (select distinct pt_hdkey from [dbo].[' + @mwPriceDataTableName + '] with(nolock) where pt_isenabled = 1 and pt_tourkey = sd_tourkey)'
							--print (@sql)
							exec (@sql)

						set @sql = 'update mwSpoDataTable set sd_isenabled = 0 where sd_key in (select xKey from #delKeys)'
						exec (@sql)

						truncate table #delKeys

						--set @sql = 'insert into #delKeys (xKey) 
						--	select td_key
						--	from tp_turdates
						--	where not exists (select 1 from ' + @mwPriceDataTableName + ' with(nolock) where pt_isenabled = 1 and pt_tourdate = td_date and pt_tourkey = td_tokey)'

						--print (@sql)
						--exec (@sql)

						--set @sql = 'delete from tp_turdates where td_key in (select xKey from #delKeys)'
						--exec (@sql)

						truncate table #delKeys

						-- для скидывания кэша
						-----------------------
						exec ClearSearchCache null, @cnKey, @ctKeyFrom
						------------------------
					end
					
					fetch next from mwPriceDataTableNameCursor into @mwPriceDataTableName, @cnKey, @ctKeyFrom;				
				end

				close mwPriceDataTableNameCursor;
				deallocate mwPriceDataTableNameCursor;
			end
			else
			begin
				--Секционирование не используется
				update dbo.mwPriceDataTable with(rowlock)
				set pt_isenabled = 0
				where exists(select 1 from #mwReplDeletedPricesTemp r where r.rdp_pricekey = pt_pricekey);
			end
		end
	end
	
	drop index x_pricekey on #mwReplDeletedPricesTemp;
	drop table #mwReplDeletedPricesTemp;
end
