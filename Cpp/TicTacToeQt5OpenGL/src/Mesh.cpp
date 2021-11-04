#include "Mesh.h"

#include <QVector3D>
#include <QVector2D>

struct VertexData
{
    QVector3D position;
    QVector2D texCoord;
    QVector3D normal;
};


Mesh::Mesh() : m_ebo(QOpenGLBuffer::IndexBuffer) {
  initializeOpenGLFunctions();

  m_vbo.create();
  m_ebo.create();

  m_vao.create();

  initGeometry();
}

Mesh::~Mesh() {
  m_vao.destroy();

  m_ebo.destroy();
  m_vbo.destroy();
}

void Mesh::bind() {
  m_vao.bind();
}


int Mesh::count() {
  return m_count;
}


void Mesh::initGeometry() {
  QVector3D normal1( 0.0f,  0.0f,  1.0f);

  QVector3D normal2( 0.0f, -1.0f,  0.0f);
  QVector3D normal3( 0.0f,  1.0f,  0.0f);

  QVector3D normal4(-1.0f,  0.0f,  0.0f);
  QVector3D normal5( 1.0f,  0.0f,  0.0f);

  QVector3D normal6( 0.0f,  0.0f, -1.0f);


  VertexData vertices[] = {
    // top
    {QVector3D(-1.0f, -1.0f,  1.0f), QVector2D(0.0f, 0.0f), normal1},
    {QVector3D( 1.0f, -1.0f,  1.0f), QVector2D(1.0f, 0.0f), normal1},
    {QVector3D(-1.0f,  1.0f,  1.0f), QVector2D(0.0f, 1.0f), normal1},
    {QVector3D( 1.0f,  1.0f,  1.0f), QVector2D(1.0f, 1.0f), normal1},

    // front
    {QVector3D(-1.0f, -1.0f,  0.0f), QVector2D(0.0f, 0.0f), normal2},
    {QVector3D( 1.0f, -1.0f,  0.0f), QVector2D(1.0f, 0.0f), normal2},
    {QVector3D(-1.0f, -1.0f,  1.0f), QVector2D(0.0f, 1.0f), normal2},
    {QVector3D( 1.0f, -1.0f,  1.0f), QVector2D(1.0f, 1.0f), normal2},

    // back
    {QVector3D(-1.0f,  1.0f,  0.0f), QVector2D(0.0f, 0.0f), normal3},
    {QVector3D( 1.0f,  1.0f,  0.0f), QVector2D(1.0f, 0.0f), normal3},
    {QVector3D(-1.0f,  1.0f,  1.0f), QVector2D(0.0f, 1.0f), normal3},
    {QVector3D( 1.0f,  1.0f,  1.0f), QVector2D(1.0f, 1.0f), normal3},


    // left
    {QVector3D(-1.0f, -1.0f,  0.0f), QVector2D(0.0f, 0.0f), normal4},
    {QVector3D(-1.0f,  1.0f,  0.0f), QVector2D(1.0f, 0.0f), normal4},
    {QVector3D(-1.0f, -1.0f,  1.0f), QVector2D(0.0f, 1.0f), normal4},
    {QVector3D(-1.0f,  1.0f,  1.0f), QVector2D(1.0f, 1.0f), normal4},

    // right
    {QVector3D( 1.0f, -1.0f,  0.0f), QVector2D(0.0f, 0.0f), normal5},
    {QVector3D( 1.0f,  1.0f,  0.0f), QVector2D(1.0f, 0.0f), normal5},
    {QVector3D( 1.0f, -1.0f,  1.0f), QVector2D(0.0f, 1.0f), normal5},
    {QVector3D( 1.0f,  1.0f,  1.0f), QVector2D(1.0f, 1.0f), normal5},

    // bottom
    {QVector3D(-1.0f, -1.0f,  0.0f), QVector2D(0.0f, 0.0f), normal6},
    {QVector3D( 1.0f, -1.0f,  0.0f), QVector2D(1.0f, 0.0f), normal6},
    {QVector3D(-1.0f,  1.0f,  0.0f), QVector2D(0.0f, 1.0f), normal6},
    {QVector3D( 1.0f,  1.0f,  0.0f), QVector2D(1.0f, 1.0f), normal6}
  };

  GLuint indices[] = {
    0, 1, 2,
    1, 3, 2,

    4, 5, 6,
    5, 7, 6,

    8, 10, 9,
    9, 10, 11,

    12, 14, 13,
    13, 14, 15,

    16, 17, 18,
    17, 19, 18,

    20, 22, 21,
    21, 22, 23
  };

  m_count = sizeof(indices)/sizeof(GLuint);


  QOpenGLVertexArrayObject::Binder binder(&m_vao);

  m_vbo.bind();
  m_vbo.allocate(vertices, sizeof(vertices));

  m_ebo.bind();
  m_ebo.allocate(indices, sizeof(indices));


  glEnableVertexAttribArray(0);
  glEnableVertexAttribArray(1);
  glEnableVertexAttribArray(2);

  size_t offset = 0;
  glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, sizeof(VertexData), reinterpret_cast<void*>(offset));

  offset += sizeof(QVector3D);
  glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, sizeof(VertexData), reinterpret_cast<void*>(offset));

  offset += sizeof(QVector2D);
  glVertexAttribPointer(2, 3, GL_FLOAT, GL_FALSE, sizeof(VertexData), reinterpret_cast<void*>(offset));
}

