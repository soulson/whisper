drop table if exists character_spell;

create table character_spell (
	id int unsigned not null auto_increment,
    character_id int unsigned not null,
    spell_id mediumint unsigned not null,
    enabled bool not null default true comment 'show in spell book',
    
    primary key (id),
    unique key idx_character_spell (character_id, spell_id),
    key idx_spell (spell_id),
    
    foreign key (character_id) references `character` (id)
);