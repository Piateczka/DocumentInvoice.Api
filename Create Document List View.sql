CREATE VIEW DocumentList
AS
SELECT 
	ROW_NUMBER() OVER(ORDER BY (SELECT 1)) as id,
	d.Id as DcoumentId,
	d.DocumentName,
	d.Container, 
	d.DocumentFileName, 
	d.Month,
	d.Year,
	dt.Tag,
	COALESCE(dt.IsActive,1) AS IsActive,
	COALESCE(CAST(CAST(dt.Version AS BIGINT)+CAST(d.Version AS BIGINT) AS RowVersion),0) AS Version
FROM Documents AS d
LEFT JOIN DocumentTag AS dt
ON d.Id = dt.DocumentId;

