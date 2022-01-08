lexer grammar ANTLRGrammarLexer;

fragment A          : ('A'|'a') ;
fragment B          : ('B'|'b') ;
fragment C          : ('C'|'c') ;
fragment D          : ('D'|'d') ;
fragment E          : ('E'|'e') ;
fragment F          : ('F'|'f') ;
fragment G          : ('G'|'g') ;
fragment H          : ('H'|'h') ;
fragment I          : ('I'|'i') ;
fragment J          : ('J'|'j') ;
fragment K          : ('K'|'k') ;
fragment L          : ('L'|'l') ;
fragment M          : ('M'|'m') ;
fragment N          : ('N'|'n') ;
fragment O          : ('O'|'o') ;
fragment P          : ('P'|'p') ;
fragment Q          : ('Q'|'q') ;
fragment R          : ('R'|'r') ;
fragment S          : ('S'|'s') ;
fragment T          : ('T'|'t') ;
fragment U          : ('U'|'u') ;
fragment V          : ('V'|'v') ;
fragment W          : ('W'|'w') ;
fragment X          : ('X'|'x') ;
fragment Y          : ('Y'|'y') ;
fragment Z          : ('Z'|'z') ;

fragment LETTER     : [a-zA-Z] ;
fragment DIGIT      : [0-9] ;

WHITESPACE          : [ \t\r\n]+    -> skip;
BLOCK_COMMENT       : '/*' .*? '*/' -> channel(HIDDEN);
LINE_COMMENT        : '//' .*? '\n' -> channel(HIDDEN);

SQUOTA_STRING       : '\'' ('\\'. | '\'\'' | ~('\'' | '\\'))* '\'';

IDENTIFIER          : (LETTER | '_' | DIGIT)+ ;
PG_MACRO_OR_SMTH    : '%' P R E C [ \t\r\n]+ IDENTIFIER ;
COLON               : ':' ;
SEMI                : ';' ;
OR                  : '|' ;
OPEN_BRACE          : '{' -> pushMode(TargetLanguageText) ;

mode TargetLanguageText;
NESTED_OPEN_BRACE   : '{' -> pushMode(TargetLanguageText) ;
CLOSE_BRACE         : '}' -> popMode ;
ANY_NON_BRACE  : . ;
