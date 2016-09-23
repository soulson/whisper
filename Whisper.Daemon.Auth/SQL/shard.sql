drop table if exists shard;

create table shard (
	id int unsigned not null auto_increment,
    name char(16) not null,
    address char(15) not null,
    port int not null,
    enabled bool not null default true comment 'if false, shard will not display in client realm list',
	type tinyint not null comment 'see ShardType enumeration for possible values',
    recommended bool not null default false comment 'client displays recommended shard with special note in realm list',
    last_ping timestamp comment 'shardd updates this timestamp on a timer to show when it is online',
    
    primary key (id),
    unique key idx_name (name)
);