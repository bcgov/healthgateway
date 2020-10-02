<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { Line, mixins } from "vue-chartjs";

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
