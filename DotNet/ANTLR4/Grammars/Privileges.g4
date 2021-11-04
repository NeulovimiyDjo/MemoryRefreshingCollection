grammar Privileges;

options {
    language=CSharp;
}
@header {
}

privileges : '{' acls+=privilege (COMMA acls+=privilege)* '}' EOF;
  
privilege
  : name=Identifier? priv=Privileges grantor=Identifier  
  | QUOTE_CHAR qname=identifier? priv=Privileges qgrantor=identifier QUOTE_CHAR
  ;

identifier : (Identifier | QuotedIdentifier);  
   
COMMA: ',';
QUOTE_CHAR : '"';

// consume terminator chars to distinct this token from an Indentifier
// strip terminators for this.Text
Privileges : '=' ([acdrtxwCDTUX] '*'?)+ '/'
    {
        String __tx = this.Text;
        this.Text = __tx.Substring(1, __tx.Length - 1);
    };

Identifier: [a-zA-Z_0-9]+;
    
QuotedIdentifier
    : '\\"' UnterminatedQuotedIdentifier '\\"'
    // unquote so that we may always call this.Text and not worry about quotes
        {
            String __tx = this.Text;
            this.Text = __tx.Substring(2, __tx.Length - 2)
                        .Replace("\\\"\\\"", "\"")
                        .Replace("\\\\", "\\");
        }
    ;
    
fragment UnterminatedQuotedIdentifier
    : 
    ( '\\"\\"'
    | '\\\\'
    | ~["\\]
    )*
    ; 