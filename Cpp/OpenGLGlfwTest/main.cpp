#include "glad/glad.h"
#include "GLFW/glfw3.h"

#include "glm/vec3.hpp"
#include "glm/mat4x4.hpp"
#include "glm/gtc/matrix_transform.hpp"
#include "glm/gtc/type_ptr.hpp"
#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"
#include "ResourceManager/ResourceHandle.h"

#include <iostream>
#include <thread>
#include <atomic>
#include <cmath>

#include <fstream>
#include <string>
#include <sstream>
#include <vector>

void processInput(GLFWwindow* window);
GLuint compileShaders();
void createRectangle1(GLuint& VAO, GLuint& VBO, GLuint& EBO);
void createRectangle2(GLuint& VAO, GLuint& VBO, GLuint& EBO);
void createMyBox(GLuint& VAO, GLuint& VBO, GLuint& EBO);
void createTexture(const ResourceHandle& texture_resource, GLuint* textures, int texture_number);

const unsigned int Initial_screen_width = 800;
const unsigned int Initial_screen_height = 600;

unsigned int screen_width = Initial_screen_width;
unsigned int screen_height = Initial_screen_height;
std::atomic<bool> size_changed = false;

class WindowHandle {
public:
  WindowHandle() {
    window = glfwCreateWindow(Initial_screen_width, Initial_screen_height, "My title", nullptr, nullptr);
    glfwSetWindowPos(window, 100, 100);
  }
  ~WindowHandle() { glfwDestroyWindow(window); }

  GLFWwindow* window;
};

GLuint VAO[3], VBO[3], EBO[3];
GLuint textures[3];

GLuint shader_program;

glm::mat4 model(1.0f);
glm::mat4 view = glm::lookAt(glm::vec3(0.0f, 0.0f, 3.0f), glm::vec3(0.0f, 0.0f, 0.0f), glm::vec3(0.0f, 1.0f, 0.0f));

struct MyStruct {
  GLuint shader_program;
};

std::chrono::high_resolution_clock::time_point last_refresh;
void drawScene(GLFWwindow* window) {
  glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

  float current_t = static_cast<float>(glfwGetTime());

  glUniform1f(glGetUniformLocation(shader_program, "u_shift"), current_t);

  model = glm::translate(glm::mat4(1.0f), glm::vec3(-1.0f, -0.8f, 0.0f));
  model = glm::rotate(model, glm::radians(current_t*30.0f), glm::vec3(0.0f, 1.0f, 0.0f));
  glUniformMatrix4fv(glGetUniformLocation(shader_program, "model"), 1, GL_FALSE, glm::value_ptr(model));

  glActiveTexture(GL_TEXTURE0 + 0);
  glBindTexture(GL_TEXTURE_2D, textures[0]);

  glActiveTexture(GL_TEXTURE0 + 1);
  glBindTexture(GL_TEXTURE_2D, textures[1]);

  glUniform1i(glGetUniformLocation(shader_program, "texture0"), 0);
  glUniform1i(glGetUniformLocation(shader_program, "texture1"), 1);

  glBindVertexArray(VAO[0]);
  glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);

  glBindVertexArray(VAO[1]);
  glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);


  model = glm::scale(glm::mat4(1.0f), glm::vec3(0.5f, 0.5f, 0.5f));
  model = glm::translate(model, glm::vec3(1.5f, 1.5f, 0.0f));
  model = glm::rotate(model, glm::radians(current_t * -30.0f), glm::vec3(0.0f, 1.0f, 0.0f));
  glUniformMatrix4fv(glGetUniformLocation(shader_program, "model"), 1, GL_FALSE, glm::value_ptr(model));

  glActiveTexture(GL_TEXTURE0 + 0);
  glBindTexture(GL_TEXTURE_2D, textures[2]);

  glActiveTexture(GL_TEXTURE0 + 1);
  glBindTexture(GL_TEXTURE_2D, textures[2]);

  glUniform1i(glGetUniformLocation(shader_program, "texture0"), 0);
  glUniform1i(glGetUniformLocation(shader_program, "texture1"), 1);

  glBindVertexArray(VAO[2]);
  glDrawElements(GL_TRIANGLES, 36, GL_UNSIGNED_INT, 0);

  glfwSwapBuffers(window);
  last_refresh = std::chrono::high_resolution_clock::now();
}

glm::mat4 projection;

void windowSizeCallback(GLFWwindow* window, int width, int height) {
  MyStruct* ptr = static_cast<MyStruct*>(glfwGetWindowUserPointer(window));

  glUniform2i(glGetUniformLocation(ptr->shader_program, "u_window_size"), width, height);
  projection = glm::perspective(glm::radians(60.0f), 1.0f * width / height, 0.1f, 10.0f);
  glUniformMatrix4fv(glGetUniformLocation(ptr->shader_program, "projection"), 1, GL_FALSE, &projection[0][0]);
}

#include <condition_variable>
std::condition_variable cv;
std::mutex mt;

void superCallback1(GLFWwindow* window, int width, int height) {
  if (height > 0) {
    screen_width = width;
    screen_height = height;
    size_changed = true;
    cv.notify_one();
  }
}


std::atomic<bool> cursor_pos_changed = false;
float last_xpos;
float last_ypos;
float pitch = 0;
float yaw = 0;
float speed = 0.5f;

void cursorPosChanged(GLFWwindow* window, double xpos, double ypos) {
  if (glfwGetMouseButton(window, GLFW_MOUSE_BUTTON_LEFT) == GLFW_PRESS) {
    yaw -= fmod((static_cast<float>(xpos) - last_xpos) * speed, 360.0f);
    pitch += fmod((static_cast<float>(ypos) - last_ypos) * speed, 360.0f);

    if (pitch > 89.9)
      pitch = 89.9f;
    else if (pitch < -89.9)
      pitch = -89.9f;

    last_xpos = static_cast<float>(xpos);
    last_ypos = static_cast<float>(ypos);

    cursor_pos_changed = true;
  } else {
    last_xpos = static_cast<float>(xpos);
    last_ypos = static_cast<float>(ypos);
  }
}

int main(int argc, char* argv[]) {

  glfwInit();
  glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
  glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
  glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

  WindowHandle window_handle;

  int width, height, nr_channels;
  ResourceHandle embedded_image("cross.png");
  unsigned char* data = stbi_load_from_memory(embedded_image.data(), static_cast<int>(embedded_image.size()), &width, &height, &nr_channels, 0);
  if (data) {
    GLFWimage image{ width, height, data };
    glfwSetWindowIcon(window_handle.window, 1, &image);
  } else {
    std::cerr << "Failed to load icon\n";
  }
  stbi_image_free(data);

  ResourceHandle main("main.cpp");
  std::cout << main.c_str();

  if (!window_handle.window) {
    std::cerr << "Failed to create GLFW window\n";
    glfwTerminate();
    std::cin.get();
    return -1;
  }



  GLFWwindow* context = glfwCreateWindow(800, 300, "sldf", nullptr, window_handle.window);
  glfwSetWindowPos(context, 1000, 100);


  glfwSetFramebufferSizeCallback(window_handle.window, superCallback1);

  std::thread thread([&window_handle, &context] {
    glfwMakeContextCurrent(window_handle.window);

    if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress)) {
      std::cerr << "Failed to initialize GLAD\n";
      glfwSetWindowShouldClose(window_handle.window, 1);
      return -1;
    }

    std::cout << "GL_VENDOR: " << glGetString(GL_VENDOR) << '\n';
    std::cout << "GL_RENDERER: " << glGetString(GL_RENDERER) << '\n';
    std::cout << "GL_VERSION: " << glGetString(GL_VERSION) << '\n';
    std::cout << "GL_SHADING_LANGUAGE_VERSION: " << glGetString(GL_SHADING_LANGUAGE_VERSION) << '\n';


    shader_program = compileShaders();


    createRectangle1(VAO[0], VBO[0], EBO[0]);
    createRectangle2(VAO[1], VBO[1], EBO[1]);

    createMyBox(VAO[2], VBO[2], EBO[2]);

    createTexture(ResourceHandle("container2.png"), textures, 0);
    createTexture(ResourceHandle("awesomeface.png"), textures, 1);
    createTexture(ResourceHandle("corten-canfranc.jpg"), textures, 2);

    glEnable(GL_DEPTH_TEST);
    //glEnable(GL_CULL_FACE);
    //glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);

    glUseProgram(shader_program);


    MyStruct my_struct;
    my_struct.shader_program = shader_program;
    glfwSetWindowUserPointer(window_handle.window, &my_struct);
    windowSizeCallback(window_handle.window, Initial_screen_width, Initial_screen_height);

    glfwSetWindowAspectRatio(window_handle.window, 4, 3);
    glfwSetWindowSizeLimits(window_handle.window, 160, 120, GLFW_DONT_CARE, GLFW_DONT_CARE);

    glfwSetCursorPosCallback(window_handle.window, cursorPosChanged);

    glUniformMatrix4fv(glGetUniformLocation(shader_program, "view"), 1, GL_FALSE, &view[0][0]);

    glClearColor(0.2f, 0.3f, 0.3f, 1.0f);

    glfwSwapInterval(0);




    glfwMakeContextCurrent(context);

    GLuint VAO2[2];
    glGenVertexArrays(2, VAO2);

    glBindVertexArray(VAO2[0]);
    glBindBuffer(GL_ARRAY_BUFFER, VBO[0]);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO[0]);
    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)0);
    glEnableVertexAttribArray(0);
    glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)(3 * sizeof(float)));
    glEnableVertexAttribArray(1);
    glVertexAttribPointer(2, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)(6 * sizeof(float)));
    glEnableVertexAttribArray(2);

    glBindVertexArray(VAO2[1]);
    glBindBuffer(GL_ARRAY_BUFFER, VBO[1]);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO[1]);
    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)0);
    glEnableVertexAttribArray(0);
    glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)(3 * sizeof(float)));
    glEnableVertexAttribArray(1);
    glVertexAttribPointer(2, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)(6 * sizeof(float)));
    glEnableVertexAttribArray(2);

    glEnable(GL_DEPTH_TEST);

    glUseProgram(shader_program);

    glClearColor(0.7f, 0.7f, 0.7f, 1.0f);

    glfwSwapInterval(0);

    while (!glfwWindowShouldClose(window_handle.window)) {
      std::chrono::high_resolution_clock::time_point current_time = std::chrono::high_resolution_clock::now();
      int64_t t = (current_time - last_refresh).count();
      if (t >= 16'000'000) {
        glfwMakeContextCurrent(window_handle.window);
        drawScene(window_handle.window);

        glfwMakeContextCurrent(context);

        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        float current_t = static_cast<float>(glfwGetTime());

        glUniform1f(glGetUniformLocation(shader_program, "u_shift"), current_t);

        glUniform2i(glGetUniformLocation(shader_program, "u_window_size"), 800, 300);
        glm::mat4 ortho_projection = glm::ortho(-2.5f, 2.5f, -2.5f * 300 / 800, 2.5f * 300 / 800, 0.1f, 10.0f);
        glUniformMatrix4fv(glGetUniformLocation(shader_program, "projection"), 1, GL_FALSE, &ortho_projection[0][0]);

        model = glm::rotate(glm::mat4(1.0f), glm::radians(current_t*30.0f), glm::vec3(0.0f, 1.0f, 0.0f));
        glUniformMatrix4fv(glGetUniformLocation(shader_program, "model"), 1, GL_FALSE, glm::value_ptr(model));

        glActiveTexture(GL_TEXTURE0 + 0);
        glBindTexture(GL_TEXTURE_2D, textures[0]);

        glActiveTexture(GL_TEXTURE0 + 1);
        glBindTexture(GL_TEXTURE_2D, textures[1]);

        glUniform1i(glGetUniformLocation(shader_program, "texture0"), 0);
        glUniform1i(glGetUniformLocation(shader_program, "texture1"), 1);

        glBindVertexArray(VAO2[0]);
        glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);

        glBindVertexArray(VAO2[1]);
        glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);

        //glBindVertexArray(VAO[2]);
        //glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);

        glfwSwapBuffers(context);
        glfwMakeContextCurrent(window_handle.window);


        GLint view_port[4];
        glGetIntegerv(GL_VIEWPORT, view_port);
        glUniform2i(glGetUniformLocation(shader_program, "u_window_size"), view_port[2], view_port[3]);

        glUniformMatrix4fv(glGetUniformLocation(shader_program, "projection"), 1, GL_FALSE, &projection[0][0]);
      }

      current_time = std::chrono::high_resolution_clock::now();
      t = (current_time - last_refresh).count();
      if (t < 16'000'000) {
        std::unique_lock<std::mutex> lk(mt);
        cv.wait_until(lk, std::chrono::high_resolution_clock::now() + std::chrono::nanoseconds(16'000'000 - t), [] { return static_cast<bool>(size_changed); });
      }

      if (size_changed) {
        glViewport(0, 0, screen_width, screen_height);
        windowSizeCallback(window_handle.window, screen_width, screen_height);
        drawScene(window_handle.window);
        size_changed = false;
      }

      if (cursor_pos_changed) {
        float x = sin(glm::radians(yaw)) * cos(glm::radians(pitch)) * 3;
        float y = sin(glm::radians(pitch)) * 3;
        float z = cos(glm::radians(yaw)) * cos(glm::radians(pitch)) * 3;

        view = glm::lookAt(glm::vec3(x, y, z), glm::vec3(0.0f, 0.0f, 0.0f), glm::vec3(0.0f, 1.0f, 0.0f));
        glUniformMatrix4fv(glGetUniformLocation(shader_program, "view"), 1, GL_FALSE, &view[0][0]);
        cursor_pos_changed = false;
      }
    }

    glDeleteVertexArrays(3, VAO);
    glDeleteBuffers(3, VBO);
    glDeleteBuffers(3, EBO);
    glDeleteTextures(2, textures);

    glDeleteProgram(shader_program);

    return 0;
  });


  while (!glfwWindowShouldClose(window_handle.window)) {
    processInput(window_handle.window);
    glfwWaitEvents();
  }
  glfwDestroyWindow(context);
  thread.join();

  glfwTerminate();
  return 0;
}

void processInput(GLFWwindow* window) {
  if (glfwGetKey(window, GLFW_KEY_X) == GLFW_PRESS)
    glfwSetWindowShouldClose(window, true);
  if (glfwGetMouseButton(window, GLFW_MOUSE_BUTTON_LEFT) == GLFW_PRESS)
    glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
  if (glfwGetMouseButton(window, GLFW_MOUSE_BUTTON_LEFT) == GLFW_RELEASE)
    glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_NORMAL);
}

GLuint compileShaders() {
  ResourceHandle vertex_shader_resource = ResourceHandle("vshader.vs");
  const char* vertex_shader_source = reinterpret_cast<const char*>(vertex_shader_resource.data());
  GLuint vertex_shader = glCreateShader(GL_VERTEX_SHADER);
  glShaderSource(vertex_shader, 1, &vertex_shader_source, nullptr);
  glCompileShader(vertex_shader);

  int success;
  char info_log[512];
  glGetShaderiv(vertex_shader, GL_COMPILE_STATUS, &success);
  if (!success) {
    glGetShaderInfoLog(vertex_shader, 512, nullptr, info_log);
    std::cerr << "ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" << info_log << '\n';
  }

  ResourceHandle fragment_shader_resource = ResourceHandle("fshader.fs");
  const char* fragment_shader_source = reinterpret_cast<const char*>(fragment_shader_resource.data());
  GLuint fragment_shader = glCreateShader(GL_FRAGMENT_SHADER);
  glShaderSource(fragment_shader, 1, &fragment_shader_source, nullptr);
  glCompileShader(fragment_shader);

  glGetShaderiv(fragment_shader, GL_COMPILE_STATUS, &success);
  if (!success) {
    glGetShaderInfoLog(fragment_shader, 512, nullptr, info_log);
    std::cerr << "ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n" << info_log << '\n';
  }


  GLuint shader_program = glCreateProgram();
  glAttachShader(shader_program, vertex_shader);
  glAttachShader(shader_program, fragment_shader);
  glLinkProgram(shader_program);

  glGetProgramiv(shader_program, GL_LINK_STATUS, &success);
  if (!success) {
    glGetProgramInfoLog(shader_program, 512, nullptr, info_log);
    std::cerr << "ERROR::SHADER::PROGRAM::LINKING_FAILED\n" << info_log << '\n';
  }

  glDetachShader(shader_program, vertex_shader);
  glDetachShader(shader_program, fragment_shader);
  glDeleteShader(vertex_shader);
  glDeleteShader(fragment_shader);

  return shader_program;
}

void createRectangle1(GLuint& VAO, GLuint& VBO, GLuint& EBO) {
  float vertices[] = {
    // positions      // colors         // texture coords
    0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, // top right
    0.5f, -0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, // bottom right
    -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, // bottom left
    -0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f // top left
  };

  unsigned int indices[] = {
    3, 1, 0,
    3, 2, 1
  };

  glGenVertexArrays(1, &VAO);
  glGenBuffers(1, &VBO);
  glGenBuffers(1, &EBO);

  glBindVertexArray(VAO);

  glBindBuffer(GL_ARRAY_BUFFER, VBO);
  glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

  glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);
  glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);

  glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)0);
  glEnableVertexAttribArray(0);

  glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)(3*sizeof(float)));
  glEnableVertexAttribArray(1);

  glVertexAttribPointer(2, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)(6 * sizeof(float)));
  glEnableVertexAttribArray(2);


  //glBindBuffer(GL_ARRAY_BUFFER, 0);

  //glBindVertexArray(0);
}

void createRectangle2(GLuint& VAO, GLuint& VBO, GLuint& EBO) {
  float vertices[] = {
    // positions      // colors         // texture coords
    0.5f, 0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, // top right
    0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, // bottom right
    -0.5f, -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, // bottom left
    -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f // top left
  };

  unsigned int indices[] = {
    0, 1, 3,
    1, 2, 3
  };

  glGenVertexArrays(1, &VAO);
  glGenBuffers(1, &VBO);
  glGenBuffers(1, &EBO);

  glBindVertexArray(VAO);

  glBindBuffer(GL_ARRAY_BUFFER, VBO);
  glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

  glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);
  glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);

  glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)0);
  glEnableVertexAttribArray(0);

  glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)(3 * sizeof(float)));
  glEnableVertexAttribArray(1);

  glVertexAttribPointer(2, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)(6 * sizeof(float)));
  glEnableVertexAttribArray(2);


  //glBindBuffer(GL_ARRAY_BUFFER, 0);

  //glBindVertexArray(0);
}


#include <unordered_map>

struct Vertex {
  glm::vec3 vertex_coords;
  glm::vec3 normal_coords;
  glm::vec2 texture_coords;
};

namespace std {
  template<> struct hash<Vertex> {
    size_t operator()(const Vertex & v) const {
      return hash<float>()(v.vertex_coords.x*7 + v.vertex_coords.y*11 + v.vertex_coords.z*13 +
        v.normal_coords.x * 17 + v.normal_coords.y * 19 + v.normal_coords.z * 23 + v.texture_coords.s*29 + v.texture_coords.t*31);
    }
  };
}

const float Delta = 0.001f;
struct VertexEqual {
  bool operator()(const Vertex& a, const Vertex& b) const {
    if (abs(a.vertex_coords.x - b.vertex_coords.x) < Delta && abs(a.vertex_coords.y - b.vertex_coords.y) < Delta &&
      abs(a.vertex_coords.z - b.vertex_coords.z) < Delta &&
      abs(a.normal_coords.x - b.normal_coords.x) < Delta && abs(a.normal_coords.y - b.normal_coords.y) < Delta &&
      abs(a.normal_coords.z - b.normal_coords.z) < Delta &&
      abs(a.texture_coords.s - b.texture_coords.s) < Delta && abs(a.texture_coords.t - b.texture_coords.t) < Delta)
      return true;
    else
      return false;
  }
};


void createMyBox(GLuint& VAO, GLuint& VBO, GLuint& EBO) {

  std::unordered_map<Vertex, unsigned int, std::hash<Vertex>, VertexEqual> vertices_map;
  std::vector<unsigned int> indices;
  {
    std::vector<glm::vec3> vertex_coords;
    std::vector<glm::vec2> texture_coords;
    std::vector<glm::vec3> normal_coords;

    std::ifstream ifs("../../textured_cube.obj");

    std::string line;
    while (std::getline(ifs, line)) {
      if (line.empty()) continue;

      std::string first_word;
      std::istringstream iss{ line };
      iss >> first_word;
      if (first_word == "#")
        continue;
      else if (first_word == "mtllib")
        continue;
      else if (first_word == "0")
        continue;
      else if (first_word == "v") {
        glm::vec3 vertex;

        iss >> vertex.x;
        iss >> vertex.y;
        iss >> vertex.z;

        vertex_coords.push_back(vertex);
      } else if (first_word == "vt") {
        glm::vec2 texture;

        iss >> texture.s;
        iss >> texture.t;

        texture_coords.push_back(texture);
      } else if (first_word == "vn") {
        glm::vec3 normal;

        iss >> normal.x;
        iss >> normal.y;
        iss >> normal.z;

        normal_coords.push_back(normal);
      } else if (first_word == "usemtl")
        continue;
      else if (first_word == "s")
        continue;
      else if (first_word == "f") {
        unsigned int v_index;
        unsigned int t_index;
        unsigned int n_index;

        for (int i = 0; i < 3; ++i) {
          iss >> v_index;
          iss.ignore(1, '/');

          iss >> t_index;
          iss.ignore(1, '/');

          iss >> n_index;


          auto emplace_result = vertices_map.emplace(
            std::pair<Vertex, unsigned int>(Vertex{ vertex_coords[v_index - 1], normal_coords[n_index - 1], texture_coords[t_index - 1] },
                                            static_cast<unsigned int>(vertices_map.size())));

          indices.push_back(static_cast<unsigned int>(emplace_result.first->second));
        }
      }
    }
  }

  std::vector<Vertex> vertices;
  vertices.resize(vertices_map.size());
  for (auto& v : vertices_map)
    vertices[v.second] = v.first;

  glGenVertexArrays(1, &VAO);
  glGenBuffers(1, &VBO);
  glGenBuffers(1, &EBO);

  glBindVertexArray(VAO);

  glBindBuffer(GL_ARRAY_BUFFER, VBO);
  glBufferData(GL_ARRAY_BUFFER, sizeof(Vertex) * vertices.size(), vertices.data(), GL_STATIC_DRAW);

  glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);
  glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(unsigned int) * indices.size(), indices.data(), GL_STATIC_DRAW);

  glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)0);
  glEnableVertexAttribArray(0);

  glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)(3 * sizeof(float)));
  glEnableVertexAttribArray(1);

  glVertexAttribPointer(2, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (GLvoid*)(6 * sizeof(float)));
  glEnableVertexAttribArray(2);


  //glBindBuffer(GL_ARRAY_BUFFER, 0);

  //glBindVertexArray(0);
}

void createTexture(const ResourceHandle& texture_resource, GLuint* textures, int texture_number) {
  glGenTextures(1, &textures[texture_number]);
  glBindTexture(GL_TEXTURE_2D, textures[texture_number]);

  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);

  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

  int width, height, nr_channels;
  stbi_set_flip_vertically_on_load(true);
  unsigned char* data = stbi_load_from_memory(texture_resource.data(), static_cast<int>(texture_resource.size()), &width, &height, &nr_channels, 0);
  if (data) {
    if (nr_channels == 4) {
      glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, data);
      glGenerateMipmap(GL_TEXTURE_2D);
    } else if (nr_channels == 3) {
      glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, data);
      glGenerateMipmap(GL_TEXTURE_2D);
    } else {
      std::cerr << "Wrong texture formant\n";
    }
  } else {
    std::cerr << "Failed to load texture\n";
  }
  stbi_image_free(data);
}
