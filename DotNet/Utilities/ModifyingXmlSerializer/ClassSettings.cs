using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyXmlSerializerProject
{
    public class ClassSettings<T> : IClassSettings
    {
        private readonly Dictionary<string, ClassMemberSettings> _settings = new();

        public Dictionary<string, ClassMemberSettings> Settings => _settings;

        public ClassSettings(Dictionary<Expression<Func<T, object>>, ClassMemberSettings> expressions)
        {
            foreach (Expression<Func<T, object>> expr in expressions.Keys)
            {
                string memberPath = GetMemberPath(expr);
                ClassMemberSettings settings = expressions[expr];
                _settings.Add(memberPath, settings);
            }
        }

        private string GetMemberPath(Expression<Func<T, object>> expr)
        {
            Stack<string> memberPathStack = new();
            MemberExpression me = (MemberExpression)expr.Body;
            while (me != null)
            {
                string propertyName = me.Member.Name;
                memberPathStack.Push(propertyName);
                me = GetParentMemberExpression(me);
            }
            return string.Join(".", memberPathStack);
        }

        private MemberExpression GetParentMemberExpression(MemberExpression me)
        {
            if (me.Expression.NodeType == ExpressionType.MemberAccess)
                return (MemberExpression)me.Expression;
            else if (me.Expression.NodeType == ExpressionType.ArrayIndex)
                return (MemberExpression)((BinaryExpression)me.Expression).Left;
            else if (me.Expression.NodeType == ExpressionType.Parameter)
                return null;
            else
                throw new ArgumentException($"Invalid me.Expression.NodeType: '{me.Expression.NodeType}'");
        }
    }
}
