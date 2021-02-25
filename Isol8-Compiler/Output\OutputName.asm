EXTERN printf :PROC
.DATA
	myInt DD 1
	helloWorld DB "Hello World", 10, 0
	myPtr DQ 0
.CODE
;START FUNCTION PROLOGUE
Initial PROC
	sub rsp, 28h
;END FUNCTION PROLOGUE

;START ASSIGNPTR ROUTINE
	push rax
	lea rax, [myInt]
	mov myPtr, rax
	pop rax
;END ASSIGNPTR ROUTINE

;START INC/ADD ROUTINE
	add [myInt], 8
;END INC/ADD ROUTINE

;START PRINTF ROUTINE
	lea rcx, [helloWorld]
	call printf
;END PRINTF ROUTINE

;START PRINTF ROUTINE
	xor edx, edx
	mov eax, [myInt]
	mov ecx, 0Ah
	div ecx
	add eax, 30h
	add eax, edx
	mov edx, [myInt]
	mov[rsp], r10
	mov[myInt], eax
	lea rcx, [myInt]
	call printf
	mov edx, [rsp + 8]
	mov[myInt], edx
;END PRINTF ROUTINE

;START DEL ROUTINE
	lea rax, myInt
	mov dword ptr [rax], 0
;END DEL ROUTINE

;START FUNCTION EPILOGUE
	mov rax, 0
	add rsp, 28h
	ret
Initial ENDP
;END FUNCTION EPILOGUE
END