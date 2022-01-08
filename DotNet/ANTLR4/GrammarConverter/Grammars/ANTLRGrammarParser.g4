parser grammar ANTLRGrammarParser;

options { tokenVocab = ANTLRGrammarLexer; }

root
    : rules EOF
    ;

rules
   : rule_spec*
   ;

rule_spec
   : rule_name COLON rule_val_list SEMI
   ;

rule_name
   : IDENTIFIER
   ;

rule_val_list
   : rule_val | rule_val_list OR rule_val
   ;

rule_val
   : val_items pg_macro_or_smth? target_lang_section?
   ;

pg_macro_or_smth
   : PG_MACRO_OR_SMTH
   ;

target_lang_section
   : OPEN_BRACE target_lang_expr CLOSE_BRACE
   ;

target_lang_expr
   : ANY_NON_BRACE*
   | target_lang_expr NESTED_OPEN_BRACE target_lang_expr CLOSE_BRACE target_lang_expr
   ;

val_items
   : /* EMPTY */
   | val_items val_item
   ;

val_item
   : IDENTIFIER
   | SQUOTA_STRING
   ;
