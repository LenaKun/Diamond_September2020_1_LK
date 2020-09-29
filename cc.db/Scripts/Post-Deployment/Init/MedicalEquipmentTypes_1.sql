
set identity_insert dbo.medicalequipmenttypes on
insert into dbo.MedicalEquipmentTypes(id, name)
select rank() over (order by name), name  from
(select 'Eye glasses ' as name
union select 'Hearing aides '
union select 'Wheel chairs '
union select 'Prosthetic devices '
union select 'Special beds '
union select 'Hospital equipment needed at home-(oxygen masks) '
union select 'Personal emergency response system/installation of such equipment (not for Blue Card clients) '
union select 'Supportive footwear '
union select 'Stockings '
union select 'Walkers/canes '
union select 'Oxygen masks '
union select 'Orthotics '
union select 'Bed pans '
union select 'Special chairs'
) as t
set identity_insert dbo.medicalequipmenttypes off