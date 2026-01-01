public static XmlSchema Schema
{
	get
	{
		string tns = "http://testtest";
		string xmlns = "http://www.w3.org/2001/XMLSchema";

		XmlSchema schema = new XmlSchema();
		schema.TargetNamespace = tns;
		schema.ElementFormDefault = XmlSchemaForm.Qualified;

		XmlSchemaElement root = new XmlSchemaElement();
		root.Name = "Product_Root";
		root.SchemaTypeName = new XmlQualifiedName("Product_Type", tns);
		schema.Items.Add(root);

		XmlSchemaComplexType productType = new XmlSchemaComplexType();
		productType.Name = "Product_Type";
		schema.Items.Add(productType);

		XmlSchemaSequence sequence = new XmlSchemaSequence();
		productType.Particle = sequence;

		XmlSchemaElement id = new XmlSchemaElement();
		id.Name = "ID";
		id.SchemaTypeName = new XmlQualifiedName("int", xmlns);
		sequence.Items.Add(id);

		XmlSchemaElement name = new XmlSchemaElement();
		name.Name = "Name";
		name.SchemaTypeName = new XmlQualifiedName("string", xmlns);
		sequence.Items.Add(name);

		return schema;
	}
}

public static XmlSchemaComplexType MySchema(XmlSchemaSet schemas)
{
	XmlSchema schema = Schema;
	schemas.Add(schema);

	return (XmlSchemaComplexType)schema.SchemaTypes[
		new XmlQualifiedName(
			"date", "http://www.w3.org/2001/XMLSchema")];
}