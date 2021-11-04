#ifndef MESH_H
#define MESH_H

#include <QOpenGLBuffer>
#include <QOpenGLVertexArrayObject>
#include <QOpenGLFunctions>


class Mesh : protected QOpenGLFunctions {
public:
  Mesh();
  ~Mesh();

  void bind();
  int count();
private:
  void initGeometry();

  QOpenGLBuffer m_vbo;
  QOpenGLBuffer m_ebo;
  QOpenGLVertexArrayObject m_vao;

  int m_count;
};

#endif // MESH_H
