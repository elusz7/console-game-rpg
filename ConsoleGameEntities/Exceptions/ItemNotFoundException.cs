﻿namespace ConsoleGameEntities.Exceptions;

public class ItemNotFoundException : InventoryException
{
    public ItemNotFoundException() : base() { }
    public ItemNotFoundException(string message) : base(message) { }
}
