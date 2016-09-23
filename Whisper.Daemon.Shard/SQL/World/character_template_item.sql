drop table if exists character_template_item;

create table character_template_item (
	id int unsigned not null auto_increment,
    race tinyint unsigned not null,
    class tinyint unsigned not null,
    item_id mediumint unsigned not null,
    quantity tinyint unsigned not null,
    
    primary key (id),
    key idx_race_class (race, class)
);