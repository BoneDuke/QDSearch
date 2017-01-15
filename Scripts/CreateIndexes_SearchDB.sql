
CREATE NONCLUSTERED INDEX [ix_sm_filter] ON [dbo].[mwSpoDataTable]
(
	[sd_ctkeyfrom] ASC,
	[sd_isenabled] ASC,
	[sd_tourvalid] ASC
)
INCLUDE ( 	[sd_cnkey],
	[sd_cnname],
	[sd_ctfromname]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [ix_sm_filter] ON [dbo].[mwPriceHotels]
(
	[sd_tourkey] ASC,
	[sd_hdkey] ASC,
	[sd_hdstars] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [ix_sm_filter2]
ON [dbo].[mwPriceHotels] ([sd_tourkey],[sd_pnkey])
INCLUDE ([sd_hdkey])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [ix_sm_filter]
ON [dbo].[HotelDictionary] ([HD_STARS],[HD_CTKEY])
INCLUDE ([HD_KEY],[HD_NAME],[HD_RSKEY],[HD_HTTP])
GO

declare @sql varchar(max), @tableName varchar(100)
declare dirCursor cursor local fast_forward for
select name from sys.objects where type like 'U' and name like 'mwPriceDataTable%'

open dirCursor
fetch dirCursor into @tableName
while (@@FETCH_STATUS = 0)
begin
	set @sql = 'DROP INDEX [ix_filter_1] ON [dbo].[' + @tableName + ']'
	begin try
		print (@sql)
		exec (@sql)
	end try
	begin catch
		print @tableName
		print 'oops!!!'
	end catch

	set @sql = 'CREATE NONCLUSTERED INDEX [ix_filter_1] ON [dbo].[' + @tableName + ']
(
	[pt_tourkey] ASC,
	[pt_tourdate] ASC,
	[pt_isenabled] ASC,
	[pt_ctkey] ASC
)
INCLUDE ( 	[pt_nights],
	[pt_hdkey],
	[pt_pnkey]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
'

	begin try
		print (@sql)
		exec (@sql)
	end try
	begin catch
		print @tableName
		print 'oops!!!'
	end catch

	set @sql = 'DROP INDEX [ix_filter_2] ON [dbo].[' + @tableName + ']'
	begin try
		print (@sql)
		exec (@sql)
	end try
	begin catch
		print @tableName
		print 'oops!!!'
	end catch

	set @sql = 'CREATE NONCLUSTERED INDEX [ix_filter_2] ON [dbo].[' + @tableName + ']
(
	[pt_tourkey] ASC,
	[pt_hdstars] ASC,
	[pt_ctkey] ASC,
	[pt_pnkey] ASC,
	[pt_isenabled] ASC
)
INCLUDE ( 	
	[pt_hdkey]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
'

	begin try
		print (@sql)
		exec (@sql)
	end try
	begin catch
		print @tableName
		print 'oops!!!'
	end catch

	fetch dirCursor into @tableName
end
close dirCursor
deallocate dirCursor


-------------------------------


declare @sql varchar(max), @tableName varchar(100)
declare dirCursor cursor local fast_forward for
select name from sys.objects where type like 'U' and name like 'mwPriceDataTable%'

open dirCursor
fetch dirCursor into @tableName
while (@@FETCH_STATUS = 0)
begin
	set @sql = 'DROP INDEX [ix_sm_filter_new_1] ON [dbo].[' + @tableName + ']'
	begin try
		print (@sql)
		exec (@sql)
	end try
	begin catch
		print @tableName
		print 'oops!!!'
	end catch

	set @sql = 'CREATE NONCLUSTERED INDEX [ix_sm_filter_new_1]
	ON [dbo].[' + @tableName + '] ([pt_tourdate],[pt_tourvalid],[pt_tourkey],[pt_ctkey],[pt_isenabled])
	INCLUDE ([pt_nights],[pt_hdkey],[pt_pnkey],[pt_hotelkeys],[pt_hdstars])
'

	begin try
		print (@sql)
		exec (@sql)
	end try
	begin catch
		print @tableName
		print 'oops!!!'
	end catch

	set @sql = 'DROP INDEX [ix_sm_search_new_1] ON [dbo].[' + @tableName + ']'
	begin try
		print (@sql)
		exec (@sql)
	end try
	begin catch
		print @tableName
		print 'oops!!!'
	end catch

	set @sql = 'CREATE NONCLUSTERED INDEX [ix_sm_search_new_1] ON [dbo].[' + @tableName + ']
(
pt_isenabled,
pt_tourvalid,
pt_tourkey,
pt_tourdate,
pt_nights,
pt_hdkey,
pt_pnkey,
pt_mainplaces,
pt_addplaces,
pt_hotelkeys
)
INCLUDE (pt_hdday, pt_hdnights, pt_hdpartnerkey, pt_pricekey, pt_cnkey, pt_ctkeyfrom, pt_ctkeyto, pt_topricefor, pt_price, pt_rate, pt_days, pt_hrkey, pt_chkey, pt_chbackkey, pt_chday, pt_chbackday, pt_chprkey, pt_chbackprkey, pt_chpkkey, pt_chbackpkkey, pt_directFlightAttribute, pt_backFlightAttribute, pt_hddetails, pt_hdstars ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
'

	begin try
		print (@sql)
		exec (@sql)
	end try
	begin catch
		print @tableName
		print 'oops!!!'
	end catch

	fetch dirCursor into @tableName
end
close dirCursor
deallocate dirCursor




----------------------------------




DROP INDEX [ix_filter_1_1] ON [dbo].[mwPriceDataTable]
GO

CREATE NONCLUSTERED INDEX [ix_filter_1_1] ON [dbo].[mwPriceDataTable]
(
	[pt_tourkey] ASC,
	[pt_tourdate] ASC,
	[pt_isenabled] ASC,
	[pt_ctkey] ASC,
	[pt_tourvalid],
	[pt_ctkeyfrom]
)
INCLUDE ( 	[pt_nights],
	[pt_hdkey],
	[pt_pnkey],
	[pt_cnkeys],
	[pt_ctkeys],
	[pt_hotelkeys],
	[pt_pansionkeys],
	[pt_hotelstars]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


DROP INDEX [ix_sm_search_new_1_1] ON [dbo].[mwPriceDataTable]
GO

CREATE NONCLUSTERED INDEX [ix_sm_search_new_1_1] ON [dbo].[mwPriceDataTable] ([pt_mainplaces],[pt_addplaces],[pt_tourdate],[pt_nights],[pt_ctkeyfrom],[pt_tourkey],[pt_pnkey],[pt_isenabled],[pt_tourvalid])
INCLUDE ([pt_days],[pt_cnkey],[pt_ctkeyto],[pt_pricekey],[pt_price],[pt_hdkey],[pt_hdpartnerkey],[pt_hdstars],[pt_hrkey],[pt_rate],[pt_chkey],[pt_chbackkey],[pt_hdday],[pt_hdnights],[pt_chday],[pt_chpkkey],[pt_chprkey],[pt_chbackday],[pt_chbackpkkey],[pt_chbackprkey],[pt_hotelkeys],[pt_key],[pt_topricefor],[pt_hddetails],[pt_directFlightAttribute],[pt_backFlightAttribute])

-------------------------------------------------------------------------

DROP INDEX [ix_sm_filter_new_1_1] ON [dbo].[mwSpoDataTable]
GO

CREATE NONCLUSTERED INDEX [ix_sm_filter_new_1_1]
ON [dbo].[mwSpoDataTable] ([sd_isenabled],[sd_tourvalid])
INCLUDE ([sd_cnkey],[sd_hotelkeys],[sd_cnkeys],[sd_ctkeyfrom],[sd_tourtype],[sd_ctkey])
GO

DROP INDEX [ix_sm_filter_new_2_1] ON [dbo].[mwSpoDataTable]
GO

CREATE NONCLUSTERED INDEX [ix_sm_filter_new_2_1]
ON [dbo].[mwSpoDataTable] ([sd_ctkeyfrom],[sd_tourtype],[sd_isenabled],[sd_tourvalid])
INCLUDE ([sd_ctkey],[sd_hotelkeys],[sd_cnkey],[sd_cnkeys],[sd_ctkeys],[sd_tourkey])
GO

DROP INDEX [ix_sm_filter_new_3_1] ON [dbo].[mwSpoDataTable]
GO

CREATE NONCLUSTERED INDEX [ix_sm_filter_new_3_1]
ON [dbo].[mwSpoDataTable] ([sd_cnkey],[sd_ctkeyfrom],[sd_tourtype],[sd_isenabled],[sd_tourvalid])
INCLUDE ([sd_ctkey],[sd_hotelkeys])
GO