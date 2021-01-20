.DATA
	exampleInt DD 65535
	one DD 1
	two DD 2
.CODE
Initial PROC
	ADD one, 1
	mov RAX, 1234Ah
	ret
Initial ENDP
GlobalMaths PROC
	mov RAX, 0Ah
	ret
GlobalMaths ENDP
END