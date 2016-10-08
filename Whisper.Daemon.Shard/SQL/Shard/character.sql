drop table if exists `character`;

create table `character` (
	id int unsigned not null auto_increment,
	account_id int unsigned not null,
	name char(12) not null,
	race tinyint unsigned not null,
	class tinyint unsigned not null,
	sex tinyint unsigned not null,
	skin tinyint unsigned not null,
	face tinyint unsigned not null,
	hair_style tinyint unsigned not null,
	hair_color tinyint unsigned not null,
	facial_hair tinyint unsigned not null,
	level tinyint unsigned not null default 1,
	position_x float not null,
	position_y float not null,
	position_z float not null,
	orientation float not null,
	map_id int unsigned not null,
    zone_id int unsigned not null,
	player_flags int unsigned not null default 0,
	fields blob(5128) comment 'a dump of the fields array that Character inherits from GameObject',

	primary key (id),
	unique key idx_name (name),
	key idx_account (account_id),
    
    foreign key (race) references wworld.race_definition (id)
);