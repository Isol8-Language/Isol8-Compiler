<!-- omit in toc -->
# ISOL8 Language Documentation

- [Documentation To-Do](#documentation-to-do)
- [Language Syntax](#language-syntax)
	- [Variable Declaration](#variable-declaration)
	- [Function Declaration](#function-declaration)
	- [Maths Operations](#maths-operations)
	- [Pointers](#pointers)
	- [Input / Output](#input--output)
	- [Deleting Variables](#deleting-variables)

## Documentation To-Do
- [ ] ...
<br>
<br>

## Language Syntax

### Variable Declaration
```
<VariableName> as <VariableType> <VariableValue>;
```
No ``create`` keyword is necessary to define variables.<br><br>

Supported variable types:
* Integer (signed 32bit) - ``int`` 
* String - ``string``
* Pointers - ``ptr`` 
* Boolean - ``bool`` 
* Byte - ``byte`` 

Booleans can be defined in multiple ways:
```
	// true/false are not case sensitive
	bool_x as bool TRUE;
	bool_y as bool false;

	// numeric values instead of true/false
	create bool_a as bool 1;
	create bool_b as bool 0;

	// range of values - these get converted into 1 or 0
	create bool_i as bool 4534;		// true
	create bool_j as bool -66;		// false
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

### Maths Operations

Valid operators:<br>
* Addition - ``+``
* Addition Assignment - ``+=``
* Subtraction - ``-``
* Multiplication - ``*``
* Division - ``/``

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
Output:
```
out(<VariableName>);
```
It is also possible to add a new line ``\n`` when using ``out()``, i.e. ``out(<variable>\n)`` .
___
### Deleting Variables
```
del <VariableName>;
```