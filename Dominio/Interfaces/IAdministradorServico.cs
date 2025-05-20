using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;

namespace MinimalApi.Dominio.Interfaces;

public interface IAdministradorSevico
{
    Administrador? Login(LoginDTO loginDTO);
}