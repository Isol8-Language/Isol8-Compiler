<!-- omit in toc -->
# ISOL8 Language Documentation

- [Documentation To-Do](#documentation-to-do)
- [ISOL8 Dependencies](#isol8-dependencies)
- [Language Syntax](#language-syntax)
	- [Variable Declaration](#variable-declaration)
	- [Function Declaration](#function-declaration)
	- [External Libraries](#external-libraries)
	- [Maths Operations](#maths-operations)
	- [Comparative Operators](#comparative-operators)
	- [Arrays](#arrays)
	- [Pointers](#pointers)
	- [Input / Output](#input--output)
	- [Deleting Variables](#deleting-variables)
	- [If Statements](#if-statements)
	- [For Loops](#for-loops)
	- [Comments](#comments)

## Documentation To-Do
- [ ] ...
<br>
<br>

## ISOL8 Dependencies
Currently recommended to have **Microsoft Visual Studio 2019 Enterprise** installed for maximum compatability.
<br>
<br>

## Language Syntax

### Variable Declaration
```
<VariableName> as <VariableType> <VariableValue>;
```

| Variable Type |ISOL8 Syntax | Signed? | Min. Value | Max. Value |
|---|---|---|:---:|:---:|
|8-bit integer|``byte``|?|0|255|
|16-bit integer|``short``|yes|-2<sup>15</sup>|2<sup>15</sup> - 1|
|32-bit integer|``int``|yes|-2<sup>31</sup>|2<sup>31</sup>-1|
|64-bit integer|``long``|yes|-2<sup>63</sup>|2<sup>63</sup>-1|
|Pointers|``ptr``|-|-|-|
|Strings|``string``|-|-|-|
|Booleans|``bool``|-|0|1|
<br>

Booleans can be defined in multiple ways:
```
// true/false are not case sensitive
bool_x as bool TRUE;
bool_y as bool false;

// numeric values instead of true/false
bool_a as bool 1;
bool_b as bool 0;

// range of values - these get converted into 1 or 0
bool_i as bool 4534;		// true
bool_j as bool -66;		// false
```
Variables can be assigned a value after declaration, the syntax and use case follow:
```
<Variable> = <Value/Variable>;
```
```
intOne as int 56;
intTwo as int 677;
intThree as int null;

intThree = intOne;
```
___
### Function Declaration
```
<FunctionName>() RET <ReturnType>
{
    ret <ReturnVariable>;
}
```
The program must contain one method/function which is called ``Initial()``, this is the entry point.
___
### External Libraries
Generic syntax for importing external libraries:
```
depend "<path to library>"
```
These should be placed at the very beginning of the file.
___
### Maths Operations

|Operator|ISOL8 Syntax|
|---|:---:|
|Addition|``+``|
|Addition Assignment|``+=``|
|Subtraction|``-``|
|Multiplication|``*``|
|Division|``/``|
<br>

Generic syntax for using maths in ISOL8 (except for the addition assignment operator) is as follows:
```
<Variable One> = <Variable Two> <operator> <Variable Three>;
```
This is also legal:
```
<Variable One> <operator> <Variable Two>;
```
This will store the result of the calculation in ``<Variable One>``.
<br> <br>

<!-- omit in toc -->
#### Addition Assignment:
```
<VariableName> += <Value>;
```
This is equivalent to ``<VariableName> = <VariableName> + <Value>;``
```
intVar as int 1;
intVar += 1;		// 'intVar' now equals 2
```
____
### Comparative Operators

|Operator|ISOL8 Syntax|
|---|:---:|
|Less than|``<``|
|Greater than|``>``|
|Less than or equal to|``<=``|
|Greater than or equal to|``>=``|
|Is equal to|``==``|
|Is not equal to|``!=``|

Generic syntax for using comparative operators in ISOL8:
```
<BooleanVariable> = <Variable One> <operator> <Variable Two>;
```
____

### Arrays
```
<Variable> as <ArrayType>[<ArrayLength>];
```
```
b as int[10];
```
___
### Pointers
```
<PointerName> = (ptr)<VariableName>;
```
___
### Input / Output
Input:
```
in(<variableName>);
```
Output:<br>
```
out(<VariableName>);
```

To use ``out()``, the required ``printf`` libraries must first be declared (see [external libraries](#external-libraries) for more):

```
depend "C:\Program Files (x86)\Windows Kits\10\Lib\10.0.18362.0\ucrt\x64\ucrt.lib"
depend "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\VC\Tools\MSVC\14.28.29333\lib\x64\msvcrt.lib"
depend "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\VC\Tools\MSVC\14.28.29333\lib\x64\legacy_stdio_definitions.lib"
depend "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\VC\Tools\MSVC\14.28.29333\lib\x64\legacy_stdio_wide_specifiers.lib"
```

Currently the ``out()`` function will treat integers as signed.<br> 
It is also possible to add a new line ``\n`` when using ``out()``, i.e. ``out(<variable>\n)`` .
___
### Deleting Variables
```
del <VariableName>;
```
___
### If Statements
```
if <value> == <value>
{
	<action>
}
```
___
### For Loops
```
for (<count>)
{
	<action>
}
```

It is possible to break out of the for loop by using the ``break`` keyword.
___
### Comments
Comment characters: ``##``

Syntax:
```
##a as int 43;
out(a/n);
```
This will result in a compiler error as ``a`` will not have been declared.

These (``##``) must be the first two characters of the line.
___