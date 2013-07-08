SELECT n.name as nodename, idt.name as idname, i.name as imagename, 
	i.filename, i.filename_medium as filenamemedium, i.filename_thumb as filenamethumb, 
	i.lake_code as lakecode, i.lake_name as lakename, i.magnification, i.notes, i.section, 
	i.site_hole as sitehole, i.submitted_by as submittedby, i.year, i.taxon, i.common_name as commonname, 
	i.family, i.ui_tags as imageuitags, 
	REPLACE(n.description, '\r\n', ' ') as `description`, 
	REPLACE(n.distinguishing_features, '\r\n', ' ') as `distinguishingfeatures`, 
	n.ui_tags as nodeuitags, lt.name as lighttypename 
FROM node n, image i, identification_type idt, light_type lt 
WHERE n.id = i.unique_identification_id and idt.id = n.identification_type_id 
	and n.class like '%Unique%' and (lt.id = i.light_type_id OR (i.light_type_id is NULL AND lt.id = 1)) 
ORDER BY i.name