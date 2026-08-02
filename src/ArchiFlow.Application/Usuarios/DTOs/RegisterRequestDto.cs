namespace ArchiFlow.Application.Usuarios.DTOs;

public record RegisterRequestDto(
    string Nome, 
    string Email, 
    string Senha, 
    string Role
);
