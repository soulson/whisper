drop table if exists race_definition;

create table race_definition (
	id tinyint unsigned not null auto_increment,
    name char(10) not null,
    faction_id int unsigned not null,
    flags int unsigned not null,
    display_id_male int unsigned not null,
    display_id_female int unsigned not null,
    language_id int unsigned not null,
    first_login_cinematic_id int unsigned not null,
    login_effect_spell_id mediumint unsigned not null,
    ressurection_sickness_spell_id mediumint unsigned not null,
    speed_modifier float not null,
    
    primary key (id),
    unique key idx_name (name)
);

insert into race_definition
(id, name, faction_id, flags, display_id_male, display_id_female, language_id, first_login_cinematic_id, login_effect_spell_id, ressurection_sickness_spell_id, speed_modifier)
values
(1, 'Human', 1, 12, 49, 50, 7, 81, 836, 15007, 1),
(2, 'Orc', 2, 12, 51, 52, 1, 21, 836, 15007, 1.1),
(3, 'Dwarf', 3, 12, 53, 54, 7, 41, 836, 15007, 1),
(4, 'Night Elf', 4, 4, 55, 56, 7, 61, 836, 15007, 1.2),
(5, 'Undead', 5, 12, 57, 58, 1, 2, 836, 15007, 1),
(6, 'Tauren', 6, 14, 59, 60, 1, 141, 836, 15007, 0.75),
(7, 'Gnome', 115, 12, 1563, 1564, 7, 101, 836, 15007, 0.8),
(8, 'Troll', 116, 14, 1478, 1479, 1, 121, 836, 15007, 1),
(9, 'Goblin', 1, 1, 1140, 1140, 7, 0, 836, 15007, 1);