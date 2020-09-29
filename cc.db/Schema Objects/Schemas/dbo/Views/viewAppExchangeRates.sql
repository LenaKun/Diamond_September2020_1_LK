CREATE VIEW [dbo].[viewAppExchangeRates]
	AS 


select	apps.id as AppId,
		CC.CurId as ToCur, 
		case when apps.CurrencyId = CC.CURID then 1 
			else coalesce(aex.Value, FEX.Value) 
			end as Value
from apps
CROSS join (select 'EUR' AS CURID UNION SELECT 'ILS' UNION SELECT 'USD' UNION SELECT 'AUD' UNION SELECT 'CAD' UNION SELECT 'GBP') AS CC
JOIN (
	select 
		funds.id as fundid, 
		funds.name as fundname, 
		funds.currencycode as fundcur, 
		fex.curid AS SourceCurId , 
		fex.value as sourceValue,
		fexx.curid as targetCurId, 
		fexx.value as targetValue,
		fexx.value / fex.value as Value
	from funds
		join fundexchangerates as fex on funds.id = fex.fundid
		join fundexchangerates as fexx on funds.id = fexx.fundid 
) AS FEX ON APPS.FundId = FEX.fundid AND CC.CURID = FEX.targetCurId AND APPS.CurrencyId = FEX.SourceCurId
left outer join appexchangerates as aex on apps.id = aex.appid AND CC.CURID = AEX.CurId