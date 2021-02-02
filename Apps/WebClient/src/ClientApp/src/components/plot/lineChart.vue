<script lang="ts">
import { Line, mixins } from "vue-chartjs";
import { Component, Prop, Vue } from "vue-property-decorator";

@Component({
    extends: Line,
    mixins: [mixins.reactiveData],
})
export default class LineChartComponent extends Vue<Line> {
    @Prop() chartdata!: Chart.ChartData;
    @Prop() options!: Chart.ChartOptions;

    constructor() {
        super();
    }

    private mounted() {
        this.renderChart(this.chartdata, this.options);
    }
}
</script>
