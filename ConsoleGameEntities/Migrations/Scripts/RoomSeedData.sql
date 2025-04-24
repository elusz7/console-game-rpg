SET IDENTITY_INSERT Rooms ON;
INSERT INTO Rooms (Id, Name, Description, NorthId, SouthId, EastId, WestId)
Values
	(1, 'Entrance', 'This is the room where your adventure begins.', 2, 3, null, null),
		--Hallway North, Treasure Room South
	(2, 'Hallway', 'A long hallway with doors on either side.', 4, 1, null, null),
		--Entrance South, Library North
	(3, 'Treasure Room', 'A room filled with treasure and monsters.', 1, null, null, null),
		--Entrance North
	(4, 'Library', 'A quiet library filled with ancient books.', null, 2, null, null);
		--Hallway South
SET IDENTITY_INSERT Rooms OFF;

UPDATE Monsters SET RoomId = 3 WHERE Id = 1;

UPDATE Players SET RoomId = 1 WHERE Id = 1;

UPDATE Players SET InventoryId = 1 WHERE Id = 1;