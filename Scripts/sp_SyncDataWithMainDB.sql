if exists (select 1 from sys.objects where name like '%SyncDataWithMainDB%' and type = 'p')
begin
	DROP PROCEDURE [dbo].[SyncDataWithMainDB]
end
GO

create PROCEDURE [dbo].[SyncDataWithMainDB]
AS
begin
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	create table #QuotaObjectKeys (xKey int)
	create table #QuotasKeys (xKey int)
	create table #AirSeasonKeys (xKey int)
	create table #CostsKeys (xKey int)

	insert into #QuotasKeys (xKey)
	select distinct QD_QTID
	from QuotaDetails 
	where QD_QTID not in (select QT_ID from Quotas)

	INSERT INTO [dbo].[Quotas]
           ([QT_ID]
           ,[QT_ByRoom]
           ,[QT_Comment]
           ,[QT_PRKey]
           ,[QT_PrtDogsKey]
           ,[QT_IsByCheckIn])
	select x.[QT_ID]
           ,x.[QT_ByRoom]
           ,x.[QT_Comment]
           ,x.[QT_PRKey]
           ,x.[QT_PrtDogsKey]
           ,x.[QT_IsByCheckIn]
	from mt.avalon.dbo.Quotas as x
	where x.QT_ID in (select xKey from #QuotasKeys)

	insert into #QuotaObjectKeys (xKey)
	select distinct SS_QOID
	from StopSales
	where SS_QOID not in (select QO_ID from QuotaObjects)
	union
	select QO_ID
	from mt.avalon.dbo.QuotaObjects
	where QO_ID not in (select QO_ID from QuotaObjects)
	and (QO_QTID in (select xKey from #QuotasKeys) or QO_QTID in (select QD_QTID from QuotaDetails))

	INSERT INTO [dbo].[QuotaObjects]
           ([QO_ID]
           ,[QO_SVKey]
           ,[QO_Code]
           ,[QO_SubCode1]
           ,[QO_SubCode2]
           ,[QO_QTID]
           ,[QO_CNKey]
           ,[QO_CTKey])
	select x.[QO_ID]
           ,x.[QO_SVKey]
           ,x.[QO_Code]
           ,x.[QO_SubCode1]
           ,x.[QO_SubCode2]
           ,x.[QO_QTID]
           ,x.[QO_CNKey]
           ,x.[QO_CTKey]
	from mt.avalon.dbo.QuotaObjects as x
	where x.QO_ID in (select xKey from #QuotaObjectKeys)

	delete from StopSales where SS_Date < dateadd(day, -1, getdate())
	delete from QuotaParts where QP_Date < dateadd(day, -1, getdate())
	delete from QuotaDetails where QD_Date < dateadd(day, -1, getdate())

	delete from QuotaObjects
	where QO_ID not in (select distinct SS_QOID from StopSales)
	and QO_QTID not in (select distinct QD_QTID from QuotaDetails)

	delete from Quotas 
	where QT_ID not in (select distinct QD_QTID from QuotaDetails)
	and QT_ID not in (select distinct QO_QTID from QuotaObjects)

	delete from tbl_Costs where CS_DateEnd < dateadd(day, -1, getdate()) or CS_CHECKINDATEEND < dateadd(day, -1, getdate())
	delete from AirSeason where AS_DATETO < dateadd(day, -1, getdate()) 

	insert into #AirSeasonKeys (xKey)
	select AS_ID
	from mt.avalon.dbo.AirSeason as x 
	where x.AS_ID not in (select AS_ID from AirSeason)
	and x.AS_DateTo > dateadd(day, -1, getdate())

	INSERT INTO [dbo].[AirSeason]
           ([AS_CHKEY]
           ,[AS_DATEFROM]
           ,[AS_DATETO]
           ,[AS_WEEK]
           ,[AS_TIMEFROM]
           ,[AS_TIMETO]
           ,[AS_NextDayArriv]
           ,[AS_ID]
           ,[AS_Remark])
	select x.[AS_CHKEY]
           ,x.[AS_DATEFROM]
           ,x.[AS_DATETO]
           ,x.[AS_WEEK]
           ,x.[AS_TIMEFROM]
           ,x.[AS_TIMETO]
           ,x.[AS_NextDayArriv]
           ,x.[AS_ID]
           ,x.[AS_Remark]
	from mt.avalon.dbo.AirSeason as x 
	where x.AS_ID in (select xKey from #AirSeasonKeys)

	insert into #CostsKeys (xKey)
	select CS_ID
	from mt.avalon.dbo.tbl_Costs as x 
	where x.CS_ID not in (select CS_ID from tbl_Costs)
	and (x.CS_DATEEND > dateadd(day, -1, getdate()) or x.CS_CHECKINDATEEND > dateadd(day, -1, getdate()))
	and x.CS_SVKey = 1

	INSERT INTO [dbo].[tbl_Costs]
           ([CS_SVKEY]
           ,[CS_CODE]
           ,[CS_SUBCODE1]
           ,[CS_SUBCODE2]
           ,[CS_PRKEY]
           ,[CS_PKKEY]
           ,[CS_DATE]
           ,[CS_DATEEND]
           ,[CS_WEEK]
           ,[CS_COSTNETTO]
           ,[CS_COST]
           ,[CS_DISCOUNT]
           ,[CS_TYPE]
           ,[CS_CREATOR]
           ,[CS_RATE]
           ,[CS_UPDDATE]
           ,[CS_LONG]
           ,[CS_BYDAY]
           ,[CS_FIRSTDAYNETTO]
           ,[CS_FIRSTDAYBRUTTO]
           ,[CS_PROFIT]
           ,[CS_CINNUM]
           ,[CS_TypeCalc]
           ,[cs_DateSellBeg]
           ,[cs_DateSellEnd]
           ,[CS_ID]
           ,[CS_CHECKINDATEBEG]
           ,[CS_CHECKINDATEEND]
           ,[CS_LONGMIN]
           ,[CS_TypeDivision]
           ,[CS_UPDUSER]
           ,[CS_TRFId]
           ,[CS_COID])
	select x.[CS_SVKEY]
           ,x.[CS_CODE]
           ,x.[CS_SUBCODE1]
           ,x.[CS_SUBCODE2]
           ,x.[CS_PRKEY]
           ,x.[CS_PKKEY]
           ,x.[CS_DATE]
           ,x.[CS_DATEEND]
           ,x.[CS_WEEK]
           ,x.[CS_COSTNETTO]
           ,x.[CS_COST]
           ,x.[CS_DISCOUNT]
           ,x.[CS_TYPE]
           ,x.[CS_CREATOR]
           ,x.[CS_RATE]
           ,x.[CS_UPDDATE]
           ,x.[CS_LONG]
           ,x.[CS_BYDAY]
           ,x.[CS_FIRSTDAYNETTO]
           ,x.[CS_FIRSTDAYBRUTTO]
           ,x.[CS_PROFIT]
           ,x.[CS_CINNUM]
           ,x.[CS_TypeCalc]
           ,x.[cs_DateSellBeg]
           ,x.[cs_DateSellEnd]
           ,x.[CS_ID]
           ,x.[CS_CHECKINDATEBEG]
           ,x.[CS_CHECKINDATEEND]
           ,x.[CS_LONGMIN]
           ,x.[CS_TypeDivision]
           ,x.[CS_UPDUSER]
           ,x.[CS_TRFId]
           ,x.[CS_COID]
	from mt.avalon.dbo.tbl_Costs as x 
	where x.CS_ID in (select xKey from #CostsKeys)

end
GO

grant exec on [dbo].[SyncDataWithMainDB] to public
GO