﻿namespace ConsoleGameEntities.Exceptions;

public class ItemEnchantmentException : Exception
{
    public ItemEnchantmentException() :base() { }
    public ItemEnchantmentException(string message) : base(message) { }
}
