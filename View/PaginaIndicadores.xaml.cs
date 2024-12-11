﻿using Extensionista.Drawables;
using Extensionista.Repositories;

namespace Extensionista.View;

public partial class PaginaIndicadores : ContentPage
{
    public PaginaIndicadores(string IdCurso)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        LinhasGrafico.IsVisible = true;
        LabelNotas.IsVisible = true;
        BarrasGrafico.IsVisible = false;
        LabelVagas.IsVisible = false;

        // Obter os dados do repositório
        var repository = new SisuModalidadesRepository();
        var modalidades = repository.ObterCursosPorIdCurso(IdCurso);

        var mapeamentoCotas = modalidades
             .Select((modalidade, index) => new { Numero = index + 1, NomeCota = modalidade.TIPO_CONCORRENCIA })
             .ToList();

        CollectionViewCotas.ItemsSource = mapeamentoCotas;

        // Configurar o drawable
        var drawable = new GraficoBarrasDrawable
        {
            Modalidades = modalidades, // Atribuir diretamente
            Rotulos = mapeamentoCotas.Select(m => m.Numero.ToString()).ToList()
        };

        // Associar ao GraphicsView e atualizar
        BarrasGrafico.Drawable = drawable;
        BarrasGrafico.Invalidate();


        var drawableL = new GraficoLinhasDrawable
        {
            ModalidadesBarras = modalidades,
            RotulosBarras = mapeamentoCotas.Select(m => m.Numero.ToString()).ToList()

        };

        // Associar ao GraphicsView
        LinhasGrafico.Drawable = drawableL;
        LinhasGrafico.Invalidate();
    }


    private async void sairIndicadores_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void radioButtonNotas_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is RadioButton radioButton)
        {
            if (radioButton == radioButtonNotas && radioButtonNotas.IsChecked)
            {
                // Exibe o gráfico de notas
                LinhasGrafico.IsVisible = true;
                LabelNotas.IsVisible = true;
                BarrasGrafico.IsVisible = false;
                LabelVagas.IsVisible = false;
            }
            else if (radioButton == radioButtonVagas && radioButtonVagas.IsChecked)
            {
                // Exibe o gráfico de vagas
                LinhasGrafico.IsVisible = false;
                LabelNotas.IsVisible = false;
                BarrasGrafico.IsVisible = true;
                LabelVagas.IsVisible = true;
            }
        }
    }
}