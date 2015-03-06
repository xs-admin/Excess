/**
derived from http://svn.r-project.org/R/trunk/src/main/gram.y
http://cran.r-project.org/doc/manuals/R-lang.html#Parser
*/
grammar R;

prog:   (   expr_or_assign (';'|NL)								    
        |   NL														
        )*															
        EOF															
    ;

expr_or_assign
    :   expr ('<-'|'='|'<<-') expr_or_assign						#Assignment
    |   expr														#ExpressionStatement
    ;

expr:   expr '[[' sublist ']' ']'									#ListAccess
    |   expr '[' sublist ']'										#Index
    |   expr '(' sublist ')'										#FunctionCall		
    |   expr ('::'|':::') expr										#Namespace
    |   expr ('$'|'@') expr											#MemberAccess		
    |   expr '^'<assoc=right> expr									#Power
    |   ('-'|'+') expr												#Sign
    |   expr ':' expr												#Sequence		
    |   expr USER_OP expr											#UserOp
    |   expr ('*'|'/') expr											#Multiplication
    |   expr ('+'|'-') expr											#Addition
    |   expr ('>'|'>='|'<'|'<='|'=='|'!=') expr						#Comparison
    |   '!' expr													#Negation
    |   expr ('&'|'&&') expr										#LogicalAnd
    |   expr ('|'|'||') expr										#LogicalOr
    |   '~' expr													#Formulae
    |   expr '~' expr												#Formulae
    |   expr ('->'|'->>'|':=') expr									#RightAssignment
    |   'function' '(' formlist? ')' expr							#Function
    |   '{' NL? exprlist '}'										#Compound
    |   'if' '(' expr ')' NL? expr									#IfStatement
    |   'if' '(' expr ')' NL? expr NL? 'else' NL? expr					#IfElseStatement
    |   'for' '(' ID 'in' expr ')' NL? expr							#ForEachStatement
    |   'while' '(' expr ')' NL? expr								#WhileStatement
    |   'repeat' (NL)* expr											#RepeatStatement
    |   '?' expr													#Help
    |   'next'														#NextStatement	
    |   'break'														#BreakStatement	
    |   '(' expr ')'												#Parenthesized
    |   ID															#Identifier
    |   STRING														#StringLiteral	
    |   HEX															#HexLiteral
    |   INT															#IntLiteral	
    |   FLOAT														#FloatLiteral	
    |   COMPLEX														#ComplexLiteral	
    |   'NULL'														#NullLiteral	
    |   'NA'														#NA
    |   'Inf'														#InfLiteral	
    |   'NaN'														#NanLiteral	
    |   'TRUE'														#TrueLiteral	
    |   'FALSE'														#FalseLiteral		
    ;																

exprlist
    :   expr_or_assign ((';'|NL) expr_or_assign?)*					#ExpressionList
    |																#Empty
    ;

formlist : form (',' form)* ;										

form:   ID															#FormIdentifier
    |   ID '=' expr													#FormAssignment
    |   '...'														#FormEllipsis
    ;

sublist : sub (',' sub)* ;
sub :   expr														#SubExpression
    |   ID '='													    #SubIncompleteAssignment
    |   ID '=' expr													#SubAssignment
    |   STRING '='													#SubIncompleteString
    |   STRING '=' expr												#SubStringAssignment
    |   'NULL' '='													#SubIncompleteNull
    |   'NULL' '=' expr												#SubNullAssignment
    |   '...'														#SubEllipsis
    |																#SubEmpty
    ;

HEX :   '0' ('x'|'X') HEXDIGIT+ [Ll]? ;

INT :   DIGIT+ [Ll]? ;

fragment
HEXDIGIT : ('0'..'9'|'a'..'f'|'A'..'F') ;

FLOAT:  DIGIT+ '.' DIGIT* EXP? [Ll]?
    |   DIGIT+ EXP? [Ll]?
    |   '.' DIGIT+ EXP? [Ll]?
    ;
fragment
DIGIT:  '0'..'9' ; 
fragment
EXP :   ('E' | 'e') ('+' | '-')? INT ;

COMPLEX
    :   INT 'i'
    |   FLOAT 'i'
    ;

STRING
    :   '"' ( ESC | ~[\\"] )*? '"'
    |   '\'' ( ESC | ~[\\'] )*? '\''
    ;

fragment
ESC :   '\\' ([abtnfrv]|'"'|'\'')
    |   UNICODE_ESCAPE
    |   HEX_ESCAPE
    |   OCTAL_ESCAPE
    ;

fragment
UNICODE_ESCAPE
    :   '\\' 'u' HEXDIGIT HEXDIGIT HEXDIGIT HEXDIGIT
    |   '\\' 'u' '{' HEXDIGIT HEXDIGIT HEXDIGIT HEXDIGIT '}'
    ;

fragment
OCTAL_ESCAPE
    :   '\\' [0-3] [0-7] [0-7]
    |   '\\' [0-7] [0-7]
    |   '\\' [0-7]
    ;

fragment
HEX_ESCAPE
    :   '\\' HEXDIGIT HEXDIGIT?
    ;

ID  :   '.' (LETTER|'_'|'.') (LETTER|DIGIT|'_'|'.')*
    |   LETTER (LETTER|DIGIT|'_'|'.')*
    ;
fragment LETTER  : [a-zA-Z] ;

USER_OP :   '%' .*? '%' ;

COMMENT :   '#' .*? '\r'? '\n' -> type(NL) ;

// Match both UNIX and Windows newlines
NL      :   '\r'? '\n' ;

WS      :   [ \t]+ -> skip ;
