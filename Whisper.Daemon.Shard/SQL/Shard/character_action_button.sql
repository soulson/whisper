drop table if exists character_action_button;

create table character_action_button (
	id int unsigned not null auto_increment,
    character_id int unsigned not null,
    button tinyint unsigned not null,
    action mediumint unsigned not null,
    type tinyint unsigned not null,
    
    primary key (id),
    unique key idx_character_button (character_id, button),
    
    foreign key (character_id) references `character` (id)
);