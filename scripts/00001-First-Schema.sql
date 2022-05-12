CREATE TABLE employees (
    emp_no      INT             NOT NULL,
    birth_date  DATE            NOT NULL,
    first_name  VARCHAR(14)     NOT NULL,
    last_name   VARCHAR(16)     NOT NULL,
    gender      ENUM ('M','F')  NOT NULL,    
    hire_date   DATE            NOT NULL,
    PRIMARY KEY (emp_no)
);

INSERT INTO `employees` VALUES
(10001,'1953-09-02','Georgi','Facello','M','1986-06-26'),
(10002,'1964-06-02','Bezalel','Simmel','F','1985-11-21'),
(10003,'1953-04-20','Anneke','Preusig','F','1989-06-02'),
(10004,'1957-05-23','Tzvetan','Zielinski','F','1989-02-10'),
(10005,'1955-01-21','Kyoichi','Maliniak','M','1989-09-12');
