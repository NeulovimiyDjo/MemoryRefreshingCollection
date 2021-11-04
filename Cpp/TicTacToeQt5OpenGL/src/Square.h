#ifndef SQUARE_H
#define SQUARE_H

#include "Mesh.h"
#include "GameEngine.h"

#include <QOpenGLTexture>
#include <QOpenGLFunctions>
#include <QMatrix4x4>
#include <vector>
#include <memory>

class QOpenGLShaderProgram;


enum class Statuses : unsigned char { Normal, Pressed, WinChain };


class Square : protected QOpenGLFunctions {
public:
  Square(Mesh* mesh, std::vector<std::unique_ptr<QOpenGLTexture>>* textures, QOpenGLShaderProgram* program, QMatrix4x4* world);

  void draw();
  void setLocalTransform(const QMatrix4x4& local);
  QMatrix4x4 getLocalTransform();
  void setStatus(Statuses status);
  void setType(SquareTypes type);
  SquareTypes getType();

private:
  Mesh* m_mesh;
  std::vector<std::unique_ptr<QOpenGLTexture>>* m_textures;
  QOpenGLTexture* m_texture0;
  QOpenGLTexture* m_texture1;
  QOpenGLShaderProgram* m_program;

  QMatrix4x4* m_world;
  QMatrix4x4 m_local;
  QMatrix4x4 m_z;

  bool m_is_chain_member = false;

  SquareTypes m_type = SquareTypes::Clear;
};

#endif // SQUARE_H
