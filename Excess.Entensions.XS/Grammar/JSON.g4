// Derived from http://github.com/antlr/grammars-v4/blob/master/json/JSON.g4
grammar JSON;

import expressions;

json: pair (',' pair)* ;

object
    :   '{' pair (',' pair)* '}'
    |   '{' '}' // empty object
    ;
    
pair:   (Identifier | StringLiteral) ':' value ;

array
    :   '[' value (',' value)* ']'
    |   '[' ']' // empty array
    ;

value
    :   object  
    |   array   
    |   expression
    ;
