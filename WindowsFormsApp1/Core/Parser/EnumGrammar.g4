grammar EnumGrammar;
options
{
language=CSharp;
}

// LEXEMES
DIGIT       : [0-9];
LETTER      : [a-z];

SPACE       : [ \t\r\n]+ -> skip;

UNDER_LINE  : '_';
COMMA       : ',';
EOS         : ';';
QOUTE       : '\'';
LPAR        : '(';
RPAR        : ')';

CREATE      : 'create' | 'CREATE';
AS          : 'as' | 'AS';
ENUM        : 'enum' | 'ENUM';
TYPE        : 'type' | 'TYPE';

// RULES
stmt        : CREATE create;

create      : TYPE type;
type        : LETTER id;

id          : LETTER id
            | AS as;

as          : ENUM enum;

enum        : LPAR open;
open        : QOUTE string;
string      : LETTER stringRem;

stringRem   : LETTER stringRem
              | QOUTE endString;

endString   : RPAR close
              | (COMMA open);

close       : EOS;
