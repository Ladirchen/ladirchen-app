using System;

namespace LadirchenApp.Domain.Tests;

public class Calculator
{
  public int Add(int a, int b)
  {

    return a + b;
  }

  public int Subtract(int a, int b)
  {

    return a - b;
  }

  public int Multiply(int a, int b)
  {

    return a * b;
  }

  public double Divide(int a, int b)
  {

    return b == 0 ? throw new DivideByZeroException() : (double)a / b;
  }
}