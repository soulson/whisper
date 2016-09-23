drop table if exists account;

create table account (
	id int unsigned not null auto_increment,
	name char(16) not null,
    enabled bool not null default true comment 'if false, account cannot be logged into',
    salt char(65) not null,
    verifier char(65) not null,
    session_key char(81),
    last_ip char(15),
    last_login timestamp,
    
    primary key (id),
    unique key idx_name (name)
);