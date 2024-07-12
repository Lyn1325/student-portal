Create table newCourse
(
    studentID bigint NOT NULL,
    Id int,
    Name nvarchar(50),
    Grade int,
    CreateDate date,
    ChangeDate date,
    CONSTRAINT FK_StudentID FOREIGN KEY (studentID) REFERENCES StudentCredential(studentID)
);

