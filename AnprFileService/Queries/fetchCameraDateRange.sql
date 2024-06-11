
-- Theres' not much to say about querying this data really. The only point of note is the use of SQLite, which
-- creates it's own issues because it's under featured.

-- We have stored our data in sqlite, which does not have Date, DateTime or Time datatypes. Text is used for all. 
-- We don't have any datatype to ensure the format of our dates. Given the situation, the best solution is to store date
-- in an int and use the between operator. Running between on an int will probably be the fastest solution.
select * from Files where CameraName = "GIBDTR1" and Date between "20140806" and "20201012"

-- Without using between
select * from Files where CameraName = "GIBDTR1" and Date >= "20140806" and Date <= "20201012"

-- If we had a real Date datatype, we'd store date only, then search without DATE() which is often considerably faster.
select * from Files where CameraName = "GIBDTR1" and Date >= "2014-08-06" and Date <= "2020-10-12"

-- An even better solution is to use "between"
-- this would be the best solution from the point of view of data integrity AND query speed.
select * from Files WHERE CameraName = "GIBDTR1" and (DATE(DateTime) between "2014-08-06" and "2024-06-30")