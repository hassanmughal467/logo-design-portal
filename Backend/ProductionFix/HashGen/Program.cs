using System;
using BCrypt.Net;

class Program
{
    static void Main() => System.Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123", 11));
}
