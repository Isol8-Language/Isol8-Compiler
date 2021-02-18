# ISOL8 Documentation
- [ISOL8 Documentation](#isol8-documentation)
	- [Documentation To-Do](#documentation-to-do)
	- [Syntax of the language](#syntax-of-the-language)
		- [Variable Declaration](#variable-declaration)
		- [Function Declaration](#function-declaration)
		- [Addition](#addition)
		- [Pointers](#pointers)
		- [Outputting to console](#outputting-to-console)
		- [Deleting variables](#deleting-variables)
	- [Test Cases](#test-cases)
		- [1. Declaring variables](#1-declaring-variables)
		- [2. Printing variables to console](#2-printing-variables-to-console)
		- [3.](#3)

## Documentation To-Do
- [ ] Increment test cases
<br>
<br>

## Syntax of the language

### Variable Declaration
```
create <VariableName> as <VariableType> <VariableValue>;
```

Supported variable types:
```
int
string
ptr
```

### Function Declaration
```
<FunctionName>() RET <ReturnType>
{
    ret <ReturnVariable>;
}
```

### Addition
```
<VariableName> += <Value>;
```
e.g. :
```
one +=1; // var "one" now has value 2
```

### Pointers
```
<PointerName> = (ptr)<VariableName);
```

### Outputting to console
```
out(<VariableName>);
```

### Deleting variables
```
del <VariableName>;
```

<br>
<br>

## Test Cases
### 1. Declaring variables

| Test Case | Code Block | Expected Result | Actual Result | Pass? (Y/N) |
|---|---|---|---|:---:|
|Empty int variable|[1.1](#11-code)|Compiles successfully|<-|Y|
|Int variable with random +ive value|[1.2](#12-code)|Compiles successfully|<-|Y|
|Int variable with random -ive value|[1.3](#13-code)|Compiles successfully|<-|Y|
|Int var with value of 2^16-1|[1.4](#14-code)|Compiles successfully|<-|Y|
|Int var with value of 2^32-1|[1.5](#15-code)|Compiles successfully|Compiler Error: TYPE_MISMATCH|N|
|Int var with value of 2^64-1|[1.6](#16-code)|Compiles successfully|Compiler Error: TYPE_MISMATCH|N|
|Int var with -ive value of 2^16-1|[1.7](#17-code)|Compiles successfully|<-|Y|
|Int var with -ive value of 2^32-1|[1.8](#18-code)|Compiles successfully|Compiler Error: TYPE_MISMATCH|N|
|Int var with -ive value of 2^64-1|[1.9](#19-code)|Compiles successfully|Compiler Error: TYPE_MISMATCH|N|
|Empty string variable|[1.10](#110-code)|Compiles successfully|Compiler Error: A2047 (empty/null string)|N|
|String variable with data|[1.11](#111-code)|Compiles successfully|<-|Y|
|Creating & assigning pointers|[1.12](#112-code)|Compiles successfully|<-|Y|
<br>

[Back to the top](#isol8-testing)

Code blocks below have the ```Initial()``` entry point redacted - it is left empty unless stated otherwise.

<!-- omit in toc -->
#### 1.1 Code
```
create testintvar as int null;
```
<!-- omit in toc -->
#### 1.2 Code
```
create testintvar as int 6432;
```
<!-- omit in toc -->
#### 1.3 Code
```
create testintvar as int -21378;
```
<!-- omit in toc -->
#### 1.4 Code
```
create testintvar as int 65535;
```
<!-- omit in toc -->
#### 1.5 Code
```
create testintvar as int 4294967295;
```
<!-- omit in toc -->
#### 1.6 Code
```
create testintvar as int 18446744073709551615;
```
<!-- omit in toc -->
#### 1.7 Code
```
create testintvar as int -65535;
```
<!-- omit in toc -->
#### 1.8 Code
```
create testintvar as int -4294967295;
```
<!-- omit in toc -->
#### 1.9 Code
```
create testintvar as int -18446744073709551615;
```
<!-- omit in toc -->
#### 1.10 Code
```
create teststringvar as string "";
```
```
create teststringvar as string null;    // this syntax does not work
```
<!-- omit in toc -->
#### 1.11 Code
```
create teststringvar as string "Hello World";
```
<!-- omit in toc -->
#### 1.12 Code
```
create testintvar as int 1;
create teststringvar as string "Hello World";
create intPtr as ptr null;
create strPtr as ptr null;


Initial() ret int
{
    // can these pointers be set before the method?
	intPtr = (ptr)testintvar;
	strPtr = (ptr)teststringvar;
	ret 0;
}
```
### 2. Printing variables to console
| Test Case | Code Block | Expected Result | Actual Result | Pass? (Y/N) |
|---|---|---|---|:---:|
|Int variable with value 4|[2.1](#21-code)|Prints "4" to console|<-|Y|
|Int variable with value 4524|[2.2](#22-code)|Prints "4524" to console|Prints "°☺", perhaps terminal encoding error?|N|
|Int variable with value 463|[2.3](#23-code)|Prints "463" to console|Prints "a", perhaps terminal encoding error?|N|
|Empty string variable|[2.4](#24-code)|Prints nothing|Compiler Error: A2047 (empty/null string)|N|
|String variable with value "Hello World"|[2.5](#25-code)|Prints "Hello World" to terminal|<-|Y|
|Pointer to int variable|[2.6](#26-code)|Prints value of int to terminal|Empty line|N|
|Pointer to string variable|[2.7](#27-code)|Prints value of string to terminal|Empty line|N|

[Back to the top](#isol8-testing)

<br>

<!-- omit in toc -->
#### 2.1 Code
```
create intVarOne as int 4;
Initial() ret int
{
	out(intVarOne);
	ret 0;
}
```
<!-- omit in toc -->
#### 2.2 Code
```
create intVarOne as int 4524;
Initial() ret int
{
	out(intVarOne);
	ret 0;
}
```
<!-- omit in toc -->
#### 2.3 Code
```
create intVarOne as int 463;
Initial() ret int
{
    out(intVarOne);
    ret 0;
}
```
<!-- omit in toc -->
#### 2.4 Code
```
create stringVarOne as string "";
Initial() ret int
{
    out(stringVarOne);
    ret 0;
}
```
<!-- omit in toc -->
#### 2.5 Code
```
create stringVarOne as string "Hello World";
Initial() ret int
{
	out(stringVarOne);
	ret 0;
}
```
<!-- omit in toc -->
#### 2.6 Code
source:
```
create intVarOne as int 5;
create intPtr as ptr null;
Initial() ret int
{
	intPtr = (ptr)intVarOne;	
	out(intPtr);
	ret 0;
}
```

generated ASM:
```as
EXTERN printf :PROC
.DATA
	intVarOne DD 5
	intPtr DQ 0
.CODE
;START FUNCTION PROLOGUE
Initial PROC
	sub rsp, 28h
;END FUNCTION PROLOGUE

;START ASSIGNPTR ROUTINE
	push rax
	lea rax, [intVarOne	]
	mov intPtr, rax
	pop rax
;END ASSIGNPTR ROUTINE

;START PRINTF ROUTINE
	lea rcx, [intPtr]
	call printf
;END PRINTF ROUTINE

;START FUNCTION EPILOGUE
	mov rax, 0
	add rsp, 28h
	ret
Initial ENDP
;END FUNCTION EPILOGUE
END
```
<!-- omit in toc -->
#### 2.7 Code
source:
```
create stringVarOne as string "Hello World 2";
create strPtr as ptr null;
Initial() ret int
{
	strPtr = (ptr)stringVarOne;	
	out(strPtr);
	ret 0;
}
```

generated ASM:
```as
EXTERN printf :PROC
.DATA
	stringVarOne DB "Hello World 2", 10, 0
	strPtr DQ 0
.CODE
;START FUNCTION PROLOGUE
Initial PROC
	sub rsp, 28h
;END FUNCTION PROLOGUE

;START ASSIGNPTR ROUTINE
	push rax
	lea rax, [stringVarOne	]
	mov strPtr, rax
	pop rax
;END ASSIGNPTR ROUTINE

;START PRINTF ROUTINE
	lea rcx, [strPtr]
	call printf
;END PRINTF ROUTINE

;START FUNCTION EPILOGUE
	mov rax, 0
	add rsp, 28h
	ret
Initial ENDP
;END FUNCTION EPILOGUE
END
```

### 3. 

| Test Case | Code Block | Expected Result | Actual Result | Pass? (Y/N) |
|---|---|---|---|:---:|

<br>
<br>

[Back to the top](#isol8-testing)