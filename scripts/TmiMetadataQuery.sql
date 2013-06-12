SELECT n.name, idt.name, i.name, i.filename, i.filename_medium, i.filename_thumb,
       i.expedition_code, i.lake_code, i.lake_name, i.magnification, i.notes, i.section, i.site_hole,
       i.submitted_by, i.year, i.taxon, i.common_name, i.family, i.ui_tags,
       REPLACE(n.description, '\r\n', ' '),
       REPLACE(n.distinguishing_features, '\r\n', ' '),
       n.ui_tags, lt.name
FROM   node n, image i, identification_type idt, light_type lt
WHERE  n.id = i.unique_identification_id and
       idt.id = n.identification_type_id and
       n.class like '%Unique%' and
       (lt.id = i.light_type_id OR (i.light_type_id is NULL AND lt.id = 1))
ORDER BY i.name
INTO OUTFILE '/tmp/TmiPivotMetadata.csv'
FIELDS TERMINATED BY ','
ENCLOSED BY '@'
LINES TERMINATED BY '\n';
