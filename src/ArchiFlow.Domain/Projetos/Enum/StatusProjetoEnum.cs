using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Projetos.Enum;

public enum StatusProjetoEnum
{
    Briefing = 0,
    Desenvolvimento = 1,
    Revisao = 2,
    Aprovacao = 3,
    Execucao = 4,
    Concluido = 5,
    Cancelado = 6
}